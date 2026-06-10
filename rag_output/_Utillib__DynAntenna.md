# Documented File: DynAntenna.cs
**Repository Path:** `_Utillib\DynAntenna.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;            //Invented to mimic (char *) for [In]  only.
    using SQLCHARPTRINOUT = IntPtr;       //Invented to mimic (char *) for [In, Out].
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
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
    /// Provides methods to select, update, fetch, insert and delete PDF TS antenna
    /// records in the database table <b>&lt;userID&gt;.ft_&lt;pdfName&gt;_ante</b>
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item> The methods in this class are used to perform operations on the
    /// ANTENNA table. The actual ANTENNA table name is a variable.</item>
    /// <item>The supported operations are: selectAntenna, fetchAntenna,
    /// updateAntenna, insertAntenna, deleteAntenna and closeAntenna.</item>
    /// <item>The caller must first call selectAntenna to select the required rows:
    /// this method simply sets up the cursor for subsequent use.</item>
    /// <item>Once selectAntenna has been called, the user must call fetchAntenna
    /// to  retreive a row.</item>
    /// <item>The methods updateAntenna, deleteAntenna and insertAntenna can only
    /// be called after a row has been fetched. Note: the code doesn't
    /// enforce this rule.</item>
    /// <item>The method closeAntenna must be called to release ODBC resources 
    /// (connection and statement handles etc) and to release the cursor back the 'pool'.</item>
    /// </list>
    /// </remarks>
    public class DynAntenna
    {
#if PINVOKE
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int ftSelectAntenna([In] string table, [In] string searchCriteria, [In] string orderBy, [In] short openFlag);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int ftFetchAntenna([In] int nCursor, [In, Out] FtAnte ftAnte, [In] SQLLEN[] nullInd);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int ftUpdateAntenna([In] int nCursor, [In, Out] FtAnte ftAnte, [In] SQLLEN[] nullInd);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int ftDeleteAntenna([In] int nCursor);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private extern static int ftCloseAntenna([In] int nCursor);

        public static int FtSelectAntenna_NATIVE(string table, string searchCriteria, string orderBy)
        {
            return ftSelectAntenna(table, searchCriteria, orderBy, 0);
        }
        public static int FtFetchAntenna_NATIVE(int nCursor, out FtAnte anteRec, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynAntenna.FtFetchAntenna_NATIVE(): Entry");

            //Create an FtAnte object to output.
            anteRec = new FtAnte();
            //Create an array of nulls to output.
            nullInd = new SQLLEN[FtAnte.NUM_COLUMNS];

            int nRet = -666;

            //The default marshalling handles everything.
            nRet = ftFetchAntenna(nCursor, anteRec, nullInd);

            //...Log2.v("\n\nDynAntenna.FtFetchAntenna_NATIVE(): Exit");
            return nRet;
        }
        public static int FtDeleteAntenna_NATIVE(int nCursor)
        {
            return ftDeleteAntenna(nCursor);
        }

        public static int FtCloseAntenna_NATIVE(int nCursor)
        {
            return ftCloseAntenna(nCursor);
        }

        public static int FtUpdateAntenna_NATIVE(int nCursor, FtAnte anteRec, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynAntenna.FtUpdateAntenna_NATIVE(): Entry");

            int nRet = -666;

            //The P/Invoke default marshalling takes care of everything.
            nRet = ftUpdateAntenna(nCursor, anteRec, nullInd);

            //...Log2.v("\n\nDynAntenna.FtUpdateAntenna()_NATIVE: Exit");
            return nRet;
        }
#endif
        //===================================================================================================================================
        public static Cursor[] cursors = Arrays.CreateArrayUsingDefaultElementConstructor<Cursor>(Constant.NUM_CURSORS_FEW);
        public static int nNextFreeCursor = 0;
        //===================================================================================================================================

        /// <summary>
        /// This method returns the integer index of the next free (available) Cursor object.
        /// </summary>
        /// <remarks>
        ///  The user is responsible for populating the values of the cursor object 
        ///  (e.g. hConn, hStmt etc). The user is also responsible for setting the field
        ///  cursorOpen to true before using it and false to release it.
        /// </remarks>
        /// <returns></returns>
        /// <para> - integer index of the next free Cursor object.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        public static int GetNextFreeCursor()
        {
            //Get next free cursor area.
            int curHandle = Error.NO_CURSOR_AVAILABLE;
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
                    // There are no more open cursors
                    //GenUtil.SetErr("dynSite01 -- No more open cursors.");
                    //return -1;
                    Application.Exit("\r\nDynAntenna.GetNextFreeCursor(): ERROR: No available cursors.");
                }
            }
            else
            {
                //	Just use the next available cursor
                curHandle = nNextFreeCursor;
                //  And increment the free cursor count.
                nNextFreeCursor++;
            }

            if (curHandle != 0)
            {
                //...Log2.v("\r\nDynAntenna.GetNextFreeCursor(): found available cursor: curHandle = " + curHandle);
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

        /// <summary>
        /// Deletes a prescribed PDF TS antenna record from the database.
        /// </summary>
        /// <param name="curHandle"> - index of the FtCursor object that encapsulates the details of the record deletion.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - deletion attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        public static int FtDeleteAntenna(int curHandle)
        {
            //...Log2.v("\n\nDynAntenna.FtDeleteAntenna(): Entry");

            SQLRETURN sqlRet;
            SQLHANDLE hConn = Ssutil.NewConn();
            SQLHANDLE hUpdate;

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (cursors[curHandle].pastLastRow)
            {
                /* Cursor is past the last row */
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_PAST_LAST_ROW);
            }

            string delete_buf = String.Format("delete from {0} where call1='{1}' and call2='{2}' and bndcde='{3}' and anum={4}",
                cursors[curHandle].tableName,
                cursors[curHandle].cCurrentCall1,
                cursors[curHandle].cCurrentCall2,
                cursors[curHandle].cCurrentBndcde,
                cursors[curHandle].nCurrentAnum
                );

            //...Log2.v("\r\nDynAntenna.FtDeleteAntenna(): SQLExecDirect(): \r\n" + delete_buf);
            sqlRet = ODBC.SQLExecDirect(hUpdate, delete_buf, delete_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hUpdate, "ftDeleteAntenna01 -- Error deleting from " + cursors[curHandle].tableName);
                Ssutil.DisConnStmt(hConn, hUpdate);
                return -1;
            }

            Ssutil.DisConnStmt(hConn, hUpdate);

            //...Log2.v("\n\nDynAntenna.FtDeleteAntenna(): Exit");
            return 0;
        }

        /// <summary>
        /// Closes (releases) a cursor associated with a PDF TS antenna record; 
        /// the ODBC statement handle is released, the ODBC connection is closed and the
        /// Cursor object's cursorOpen field is set to false.
        /// </summary>
        /// <param name="curHandle"> - index of the Cursor object.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - successful outcome.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - Cursor field cursorOpen is set to false.</para>
        public static int FtCloseAntenna(int curHandle)
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

            // Close the antenna cursor.

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
        /// Updates the fields in a PDF TS ANTENNA record to mirror those prescribed in
        /// a FtAnte object.
        /// </summary>
        /// <param name="curHandle">- index of the FtCursor object.</param>
        /// <param name="ftAnte"> - FtAnte object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pAnte.</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - update attempt was successful.</para>
        /// <para>- ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>- ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>- ErrorMessages.ODBC_BINDING_ERROR - attempt to create an ODBC parameter binding failed.  </para>
        /// <para>- ErrorMessages.ODBC_QUERY_FAILED - update attempt failed - ODBC diagnostic information will be written to output.</para>
        public static int FtUpdateAntenna(int curHandle, FtAnte ftAnte, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynAntenna.FtUpdateAntenna(): Entry");
            //...Log2.v("\n\nDynAntenna.FtUpdateAntenna(): ftAnte: cmd = " + ftAnte.cmd + ";   " + ftAnte.KeysToString());
             
            //char update_buf[STMT_BUF_SZ];
            string update_buf;

            //SQLRETURN sqlRet = 0;
            SQLHANDLE hUpdate;

            SQLHANDLE hConn = Ssutil.NewConn();
            if (hConn == SQLHANDLE.Zero)
            {
                Application.Exit("\r\nDynAntenna.FtUpdateAntenna(): hConn = Ssutil.NewConn() FAILED");
            }

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hUpdate);

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (cursors[curHandle].pastLastRow)
            {
                /* Cursor is past the last row */
                Ssutil.DisConnStmt(hConn, hUpdate);
                return (Error.DYN_PAST_LAST_ROW);
            }

            //Prepare the update statement.

            //First construct the SQL 'where' clause.
            StringBuilder whereSB = new StringBuilder();
            whereSB.Append(" where call1='");
            whereSB.Append(cursors[curHandle].cCurrentCall1);
            whereSB.Append("' and ");
            whereSB.Append(" call2='");
            whereSB.Append(cursors[curHandle].cCurrentCall2);
            whereSB.Append("' and ");
            whereSB.Append(" bndcde='");
            whereSB.Append(cursors[curHandle].cCurrentBndcde);
            whereSB.Append("' and ");
            whereSB.Append(" anum=");
            whereSB.Append(cursors[curHandle].nCurrentAnum);

            //sprintf_s(update_buf, sizeof(update_buf), fmt_update,
            //          cursors[curHandle].tableName, cursors[curHandle].cCurrentCall1,
            //          cursors[curHandle].cCurrentCall2, cursors[curHandle].cCurrentBndcde,
            //          cursors[curHandle].nCurrentAnum);
            StringBuilder sb = new StringBuilder();
            sb.Append("update ");
            sb.Append(cursors[curHandle].tableName);
            sb.Append(" set ");
            sb.Append(FtAnte.AllColumnsForSqlUpdateAsBindings);
            sb.Append(whereSB);

            update_buf = sb.ToString();


            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            //into global memory with an SQLPOINTER pointer assigned to each one. FtAnte provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = ftAnte.CopyToArrayOfSQLPOINTERs();

            // Bind the parameters.
            try
            {
                //This will be used to enumerate the binding sequence; ODBC definition is that first binding is N = 1;
                int N;

                //   dbBindString(hUpdate, 1, "cmd", pAnte->cmd, sizeof(pAnte->cmd), nullInd + FT_ANTE_CMD);
                N = 1;
                Ssutil.DbBindStringInput(hUpdate, N, "cmd", parameterValuePtr[N - 1], (SQLULEN)ftAnte.cmd.Length, nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 2, "recstat", pAnte->recstat, sizeof(pAnte->recstat), nullInd + FT_ANTE_RECSTAT);
                N = 2;
                Ssutil.DbBindStringInput(hUpdate, N, "recstat", parameterValuePtr[N - 1], (SQLULEN)ftAnte.recstat.Length, nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 3, "call1", pAnte->call1, sizeof(pAnte->call1), nullInd + FT_ANTE_CALL1);
                N = 3;
                Ssutil.DbBindStringInput(hUpdate, N, "call1", parameterValuePtr[N - 1], (SQLULEN)ftAnte.call1.Length, nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 4, "call2", pAnte->call2, sizeof(pAnte->call2), nullInd + FT_ANTE_CALL2);
                N = 4;
                Ssutil.DbBindStringInput(hUpdate, N, "call2", parameterValuePtr[N - 1], (SQLULEN)ftAnte.call2.Length, nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 5, "bndcde", pAnte->bndcde, sizeof(pAnte->bndcde), nullInd + FT_ANTE_BNDCDE);
                N = 5;
                Ssutil.DbBindStringInput(hUpdate, N, "bndcde", parameterValuePtr[N - 1], (SQLULEN)ftAnte.bndcde.Length, nullIndPtr[N - 1]);

                //    dbBindSShort(hUpdate, 6, "anum", &pAnte->anum, nullInd + FT_ANTE_ANUM);
                N = 6;
                Ssutil.DbBindShortInput(hUpdate, N, "anum", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 7, "ause", pAnte->ause, sizeof(pAnte->ause), nullInd + FT_ANTE_AUSE);
                N = 7;
                Ssutil.DbBindStringInput(hUpdate, N, "ause", parameterValuePtr[N - 1], (SQLULEN)ftAnte.ause.Length, nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 8, "acode", pAnte->acode, sizeof(pAnte->acode), nullInd + FT_ANTE_ACODE);
                N = 8;
                Ssutil.DbBindStringInput(hUpdate, N, "acode", parameterValuePtr[N - 1], (SQLULEN)ftAnte.acode.Length, nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 9, "aht", &pAnte->aht, nullInd + FT_ANTE_AHT);
                N = 9;
                Ssutil.DbBindFloatInput(hUpdate, N, "aht", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 10, "azmth", &pAnte->azmth, nullInd + FT_ANTE_AZMTH);
                N = 10;
                Ssutil.DbBindFloatInput(hUpdate, N, "azmth", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 11, "elvtn", &pAnte->elvtn, nullInd + FT_ANTE_ELVTN);
                N = 11;
                Ssutil.DbBindFloatInput(hUpdate, N, "elvtn", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 12, "dist", &pAnte->dist, nullInd + FT_ANTE_DIST);
                N = 12;
                Ssutil.DbBindFloatInput(hUpdate, N, "dist", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 13, "offazm", pAnte->offazm, sizeof(pAnte->offazm), nullInd + FT_ANTE_OFFAZM);
                N = 13;
                Ssutil.DbBindStringInput(hUpdate, N, "offazm", parameterValuePtr[N - 1], (SQLULEN)ftAnte.offazm.Length, nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 14, "tazmth", &pAnte->tazmth, nullInd + FT_ANTE_TAZMTH);
                N = 14;
                Ssutil.DbBindFloatInput(hUpdate, N, "tazmth", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 15, "telvtn", &pAnte->telvtn, nullInd + FT_ANTE_TELVTN);
                N = 15;
                Ssutil.DbBindFloatInput(hUpdate, N, "telvtn", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 16, "tgain", &pAnte->tgain, nullInd + FT_ANTE_TGAIN);
                N = 16;
                Ssutil.DbBindFloatInput(hUpdate, N, "tgain", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 17, "txfdlnth", pAnte->txfdlnth, sizeof(pAnte->txfdlnth), nullInd + FT_ANTE_TXFDLNTH);
                N = 17;
                Ssutil.DbBindStringInput(hUpdate, N, "txfdlnth", parameterValuePtr[N - 1], (SQLULEN)ftAnte.txfdlnth.Length, nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 18, "txfdlnlh", &pAnte->txfdlnlh, nullInd + FT_ANTE_TXFDLNLH);
                N = 18;
                Ssutil.DbBindFloatInput(hUpdate, N, "txfdlnlh", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 19, "txfdlntv", pAnte->txfdlntv, sizeof(pAnte->txfdlntv), nullInd + FT_ANTE_TXFDLNTV);
                N = 19;
                Ssutil.DbBindStringInput(hUpdate, N, "txfdlntv", parameterValuePtr[N - 1], (SQLULEN)ftAnte.txfdlntv.Length, nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 20, "txfdlnlv", &pAnte->txfdlnlv, nullInd + FT_ANTE_TXFDLNLV);
                N = 20;
                Ssutil.DbBindFloatInput(hUpdate, N, "txfdlnlv", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 21, "rxfdlnth", pAnte->rxfdlnth, sizeof(pAnte->rxfdlnth), nullInd + FT_ANTE_RXFDLNTH);
                N = 21;
                Ssutil.DbBindStringInput(hUpdate, N, "rxfdlnth", parameterValuePtr[N - 1], (SQLULEN)ftAnte.rxfdlnth.Length, nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 22, "rxfdlnlh", &pAnte->rxfdlnlh, nullInd + FT_ANTE_RXFDLNLH);
                N = 22;
                Ssutil.DbBindFloatInput(hUpdate, N, "rxfdlnlh", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 23, "rxfdlntv", pAnte->rxfdlntv, sizeof(pAnte->rxfdlntv), nullInd + FT_ANTE_RXFDLNTV);
                N = 23;
                Ssutil.DbBindStringInput(hUpdate, N, "rxfdlntv", parameterValuePtr[N - 1], (SQLULEN)ftAnte.rxfdlntv.Length, nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 24, "rxfdlnlv", &pAnte->rxfdlnlv, nullInd + FT_ANTE_RXFDLNLV);
                N = 24;
                Ssutil.DbBindFloatInput(hUpdate, N, "rxfdlnlv", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 25, "txpadpam", &pAnte->txpadpam, nullInd + FT_ANTE_TXPADPAM);
                N = 25;
                Ssutil.DbBindFloatInput(hUpdate, N, "txpadpam", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 26, "rxpadlna", &pAnte->rxpadlna, nullInd + FT_ANTE_RXPADLNA);
                N = 26;
                Ssutil.DbBindFloatInput(hUpdate, N, "rxpadlna", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 27, "txcompl", &pAnte->txcompl, nullInd + FT_ANTE_TXCOMPL);
                N = 27;
                Ssutil.DbBindFloatInput(hUpdate, N, "txcompl", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 28, "rxcompl", &pAnte->rxcompl, nullInd + FT_ANTE_RXCOMPL);
                N = 28;
                Ssutil.DbBindFloatInput(hUpdate, N, "rxcompl", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 29, "obsloss", &pAnte->obsloss, nullInd + FT_ANTE_OBSLOSS);
                N = 29;
                Ssutil.DbBindFloatInput(hUpdate, N, "obsloss", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindFloat(hUpdate, 30, "kvalue", &pAnte->kvalue, nullInd + FT_ANTE_KVALUE);
                N = 30;
                Ssutil.DbBindFloatInput(hUpdate, N, "kvalue", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindSShort(hUpdate, 31, "atwrno", &pAnte->atwrno, nullInd + FT_ANTE_ATWRNO);
                N = 31;
                Ssutil.DbBindShortInput(hUpdate, N, "atwrno", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 32, "nota", pAnte->nota, sizeof(pAnte->nota), nullInd + FT_ANTE_NOTA);
                N = 32;
                Ssutil.DbBindStringInput(hUpdate, N, "nota", parameterValuePtr[N - 1], (SQLULEN)ftAnte.nota.Length, nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 33, "apoint", pAnte->apoint, sizeof(pAnte->apoint), nullInd + FT_ANTE_APOINT);
                N = 33;
                Ssutil.DbBindStringInput(hUpdate, N, "apoint", parameterValuePtr[N - 1], (SQLULEN)ftAnte.apoint.Length, nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 34, "sdate", pAnte->sdate, sizeof(pAnte->sdate), nullInd + FT_ANTE_SDATE);
                N = 34;
                Ssutil.DbBindStringInput(hUpdate, N, "sdate", parameterValuePtr[N - 1], (SQLULEN)ftAnte.sdate.Length, nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 35, "licence", pAnte->licence, sizeof(pAnte->licence), nullInd + FT_ANTE_LICENCE);
                N = 35;
                Ssutil.DbBindStringInput(hUpdate, N, "licence", parameterValuePtr[N - 1], (SQLULEN)ftAnte.licence.Length, nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 36, "mdate", pAnte->mdate, sizeof(pAnte->mdate), nullInd + FT_ANTE_MDATE);
                N = 36;
                Ssutil.DbBindStringInput(hUpdate, N, "mdate", parameterValuePtr[N - 1], (SQLULEN)ftAnte.mdate.Length, nullIndPtr[N - 1]);

                //    dbBindString(hUpdate, 37, "mtime", pAnte->mtime, sizeof(pAnte->mtime), nullInd + FT_ANTE_MTIME);
                N = 37;
                Ssutil.DbBindStringInput(hUpdate, N, "mtime", parameterValuePtr[N - 1], (SQLULEN)ftAnte.mtime.Length, nullIndPtr[N - 1]);
            }
            catch (Exception e)
            {
                Ssutil.DbGetDiagStmt(hUpdate, "dynAntenna03 -- Error binding parameters for site on field: " + e.Message);
                Ssutil.DisConnStmt(hConn, hUpdate);
                return Error.ODBC_BINDING_FAILED;
            }

            //...Log2.v("\r\nDynAntenna.FtUpdateAntenna(): SQLExecDirect():\r\n" + update_buf);
            SQLRETURN sqlRet;
            sqlRet = ODBC.SQLExecDirect(hUpdate, update_buf, update_buf.Length);

            //...Log2.v("\r\nDynAntenna.FtUpdateAntenna(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = "dynAntenna04 -- Error writing Antenna: " + ftAnte.call1 + " " + ftAnte.call2 + " " + ftAnte.anum;
                Ssutil.DbGetDiagStmt(hUpdate, str);
                //...Log2.v(str);
                Ssutil.DisConnStmt(hConn, hUpdate);

                return Error.ODBC_QUERY_FAILED;
            }

            //This method call releases hUpdate, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hUpdate);

            //...Log2.v("\n\nDynAntenna.FtUpdateAntenna(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Retrieves a single row of antenna data from a table in the database using 
        /// the FtCursor object created by a previous call to FtSelectAntenna().
        /// </summary>
        /// <param name="curHandle"> - FtCursor object.</param>
        /// <param name="anteRec"> - a FtAnte object populated with data from the row.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for anteRec.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
        /// <para>-   ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
        public static int FtFetchAntenna(int curHandle, out FtAnte anteRec, out SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynAntenna.FtFetchAntenna(): Entry:  cursor = \r\n" + cursors[curHandle].ToString());
            //...Log2.v("\n\nDynAntenna.FtFetchAntenna(): Entry:");

            //Create an FtAnte object to output.
            anteRec = new FtAnte();
            //Create an array of nulls to output.
            //nullInd = new SQLLEN[FtAnte.NUM_COLUMNS];
            nullInd = NullHelper.CreateArrayOfNullInd(FtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt = cursors[curHandle].hStmt;

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                //...Log2.v("\n\nDynAntenna.FtFetchAntenna(): Exit: FAIL: if (!cursors[curHandle].cursorOpen)");
                return (Error.DYN_CUR_NOT_OPEN);
            }

            // memset(anteRec, 0, sizeof(struct ftAnte_));		/* clear to be safe */
            anteRec.Initialize();

            // Fetch one row from the cursor.
            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    //...Log2.v("\n\nDynAntenna.FtFetchAntenna(): Exit: ODBC.SQLFetch(): sqlRet == ODBC.SQL_NO_DATA");
                    return (int)sqlRet;
                }
                else
                {
                    //	Error
                    //...Log2.v("\n\nDynAntenna.FtFetchAntenna(): Exit: ODBC.SQLFetch(): FAIL: sqlRet = " + sqlRet);
                    return -1;
                }
            }

            //...Log2.v("\r\nDynAntenna.FtFetchAntenna(): ODBC.SQLFetch(): SUCCEEDED");

            // Now read in the fields
            try
            {
                int colNum = 1;
                Ssutil.DbGetString(hStmt, colNum, "cmd", out anteRec.cmd, Constant.CMD_SZ, out nullInd[colNum - 1]);
                colNum = 2;
                Ssutil.DbGetString(hStmt, colNum, "recstat", out anteRec.recstat, Constant.RECSTAT_SZ, out nullInd[colNum - 1]);
                colNum = 3;
                Ssutil.DbGetString(hStmt, colNum, "call1", out anteRec.call1, Constant.CALL_SZ, out nullInd[colNum - 1]);
                colNum = 4;
                Ssutil.DbGetString(hStmt, colNum, "call2", out anteRec.call2, Constant.CALL_SZ, out nullInd[colNum - 1]);
                colNum = 5;
                Ssutil.DbGetString(hStmt, colNum, "bndcde", out anteRec.bndcde, Constant.BNDCDE_SZ, out nullInd[colNum - 1]);
                colNum = 6;
                Ssutil.DbGetShort(hStmt, colNum, "anum", out anteRec.anum, out nullInd[colNum - 1]);
                colNum = 7;
                Ssutil.DbGetString(hStmt, colNum, "ause", out anteRec.ause, Constant.AUSE_SZ, out nullInd[colNum - 1]);
                colNum = 8;
                Ssutil.DbGetString(hStmt, colNum, "acode", out anteRec.acode, Constant.ACODE_SZ, out nullInd[colNum - 1]);
                colNum = 9;
                Ssutil.DbGetFloat(hStmt, colNum, "aht", out anteRec.aht, out nullInd[colNum - 1]);
                colNum = 10;
                Ssutil.DbGetFloat(hStmt, colNum, "azmth", out anteRec.azmth, out nullInd[colNum - 1]);
                colNum = 11;
                Ssutil.DbGetFloat(hStmt, colNum, "elvtn", out anteRec.elvtn, out nullInd[colNum - 1]);
                colNum = 12;
                Ssutil.DbGetFloat(hStmt, colNum, "dist", out anteRec.dist, out nullInd[colNum - 1]);
                colNum = 13;
                Ssutil.DbGetString(hStmt, colNum, "offazm", out anteRec.offazm, Constant.OFFAZM_SZ, out nullInd[colNum - 1]);
                colNum = 14;
                Ssutil.DbGetFloat(hStmt, colNum, "tazmth", out anteRec.tazmth, out nullInd[colNum - 1]);
                colNum = 15;
                Ssutil.DbGetFloat(hStmt, colNum, "telvtn", out anteRec.telvtn, out nullInd[colNum - 1]);
                colNum = 16;
                Ssutil.DbGetFloat(hStmt, colNum, "tgain", out anteRec.tgain, out nullInd[colNum - 1]);
                colNum = 17;
                Ssutil.DbGetString(hStmt, colNum, "txfdlnth", out anteRec.txfdlnth, Constant.XFDLN_SZ, out nullInd[colNum - 1]);
                colNum = 18;
                Ssutil.DbGetFloat(hStmt, colNum, "txfdlnlh", out anteRec.txfdlnlh, out nullInd[colNum - 1]);
                colNum = 19;
                Ssutil.DbGetString(hStmt, colNum, "txfdlntv", out anteRec.txfdlntv, Constant.XFDLN_SZ, out nullInd[colNum - 1]);
                colNum = 20;
                Ssutil.DbGetFloat(hStmt, colNum, "txfdlnlv", out anteRec.txfdlnlv, out nullInd[colNum - 1]);
                colNum = 21;
                Ssutil.DbGetString(hStmt, colNum, "rxfdlnth", out anteRec.rxfdlnth, Constant.XFDLN_SZ, out nullInd[colNum - 1]);
                colNum = 22;
                Ssutil.DbGetFloat(hStmt, colNum, "rxfdlnlh", out anteRec.rxfdlnlh, out nullInd[colNum - 1]);
                colNum = 23;
                Ssutil.DbGetString(hStmt, colNum, "rxfdlntv", out anteRec.rxfdlntv, Constant.XFDLN_SZ, out nullInd[colNum - 1]);
                colNum = 24;
                Ssutil.DbGetFloat(hStmt, colNum, "rxfdlnlv", out anteRec.rxfdlnlv, out nullInd[colNum - 1]);
                colNum = 25;
                Ssutil.DbGetFloat(hStmt, colNum, "txpadpam", out anteRec.txpadpam, out nullInd[colNum - 1]);
                colNum = 26;
                Ssutil.DbGetFloat(hStmt, colNum, "rxpadlna", out anteRec.rxpadlna, out nullInd[colNum - 1]);
                colNum = 27;
                Ssutil.DbGetFloat(hStmt, colNum, "txcompl", out anteRec.txcompl, out nullInd[colNum - 1]);
                colNum = 28;
                Ssutil.DbGetFloat(hStmt, colNum, "rxcompl", out anteRec.rxcompl, out nullInd[colNum - 1]);
                colNum = 29;
                Ssutil.DbGetFloat(hStmt, colNum, "obsloss", out anteRec.obsloss, out nullInd[colNum - 1]);
                colNum = 30;
                Ssutil.DbGetFloat(hStmt, colNum, "kvalue", out anteRec.kvalue, out nullInd[colNum - 1]);
                colNum = 31;
                Ssutil.DbGetShort(hStmt, colNum, "atwrno", out anteRec.atwrno, out nullInd[colNum - 1]);
                colNum = 32;
                Ssutil.DbGetString(hStmt, colNum, "nota", out anteRec.nota, Constant.NOTA_SZ, out nullInd[colNum - 1]);
                colNum = 33;
                Ssutil.DbGetString(hStmt, colNum, "apoint", out anteRec.apoint, Constant.APOINT_SZ, out nullInd[colNum - 1]);
                colNum = 34;
                Ssutil.DbGetString(hStmt, colNum, "sdate", out anteRec.sdate, Constant.DATE_SZ, out nullInd[colNum - 1]);
                colNum = 35;
                Ssutil.DbGetString(hStmt, colNum, "licence", out anteRec.licence, Constant.LICENCE_SZ, out nullInd[colNum - 1]);
                colNum = 36;
                Ssutil.DbGetString(hStmt, colNum, "mdate", out anteRec.mdate, Constant.DATE_SZ, out nullInd[colNum - 1]);
                colNum = 37;
                Ssutil.DbGetString(hStmt, colNum, "mtime", out anteRec.mtime, Constant.TIME_SZ, out nullInd[colNum - 1]);
            }
            catch (Exception e)
            {
                //...Log2.v("\n\nDynAntenna.FtFetchAntenna(): Exit: Exception caught: " + e.Message);
                //      seterr("dynAntenna02 -- Could not get antenna, because of field %s", cName);
                GenUtil.SetErr("dynAntenna02 -- Could not get antenna, because of field " + e.Message);
                return Error.ODBC_GET_FAILED;
            }

            //...Log2.v(anteRec.ToString());

            //	Save the key for deletes.
            //    strcpy_s(cursors[curHandle].cCurrentCall1, DB_CALL_SZ, anteRec->call1);
            cursors[curHandle].cCurrentCall1 = anteRec.call1;
            //    strcpy_s(cursors[curHandle].cCurrentCall2, DB_CALL_SZ, anteRec->call2);
            cursors[curHandle].cCurrentCall2 = anteRec.call2;
            //    strcpy_s(cursors[curHandle].cCurrentBndcde, 5, anteRec->bndcde);
            cursors[curHandle].cCurrentBndcde = anteRec.bndcde;
            //cursors[curHandle].nCurrentAnum = anteRec->anum;
            cursors[curHandle].nCurrentAnum = anteRec.anum;

            //...Log2.v("\n\nDynAntenna.FtFetchAntenna(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// Selects antennae records from an antenna table in the database for
        /// the prescribed site table name, SQL search criteria and ordering clauses; the 
        /// method returns an index to a FtCursor object that can be used by subsequent
        /// calls to FtFetchAntenna().
        /// </summary>
        /// <remarks>
        /// If searchCriteria is NULL then all the rows are retrieved.
        /// If orderBy is NULL then the cursor is set for UPDATE.
        /// </remarks>
        /// <param name="table"> - full name of the database table.</param>
        /// <param name="searchCriteria"> - SQL search criteria to follow the 'where' keyword.</param>
        /// <param name="orderBy"> - SQL ordering criteria to follow the 'order by' keywords.</param>
        /// <returns></returns>
        /// <para>- non-negative value - the index of the cursor to be used for FtFetchAntenna() calls.</para>
        /// <para>- ErrorMessages.NO_CURSOR_AVAILABLE - reached limit for the number of cursors that can be open concurrently.</para>
        /// <para>-   ErrorMessages.ODBC_EXECUTE_FAILED    - call to ODBC.SQLExecute() failed to return data. </para>
        public static int FtSelectAntenna(string table, string searchCriteria, string orderBy)
        {
            //...Log2.v("\n\nDynAntenna.FtSelectAntenna(): Entry");
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            int curHandle;          /* cursor handle */

            //Construct the select clause.
            //sprintf_s(stmt_buf, sizeof(stmt_buf), select, table);
            string stmt_buf = "select " + FtAnte.AllColumnsForSqlSelect + " from " + table;

            //Construct the 'where' part of the select clause.
            //if (searchCriteria != NULL && *searchCriteria != '\0')
            if (!String.IsNullOrWhiteSpace(searchCriteria))
            {
                //Search criteria was specified.
                //strcat_s(stmt_buf, sizeof(stmt_buf), "where ");
                //strcat_s(stmt_buf, sizeof(stmt_buf), searchCriteria);
                stmt_buf += " where " + searchCriteria;
            }

            //If "order by" is not specified then assume it's for update.
            //if (orderBy == NULL || *orderBy == '\0')
            if (String.IsNullOrWhiteSpace(orderBy))
            {
                // Construct the 'for update' part of the select clause. */
                // strcat_s(stmt_buf, sizeof(stmt_buf), forUpdate);  <-- this is commmented out in FCSAwin?!
            }
            else
            {
                // Order by was specified.
                //strcat_s(stmt_buf, sizeof(stmt_buf), " order by ");
                //strcat_s(stmt_buf, sizeof(stmt_buf), orderBy);
                stmt_buf += " order by " + orderBy;
            }

            //...Log2.v("\r\nDynAntenna.FtSelectAntenna(): stmt_buf = \r\n" + stmt_buf);

            //Append the for update clause, as we want it in general.
            //safecat(stmt_buf, cAntUpdate, sizeof(stmt_buf));  <-- this is commmented out in FCSAwin?!

            //Get next free cursor area.
            curHandle = -1;
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
                    // There are no more open cursors
                    GenUtil.SetErr("dynAntenna01 -- No more open cursors.");
                    return Error.NO_CURSOR_AVAILABLE;
                }
            }
            else
            {
                //	Just use the next available cursor
                curHandle = nNextFreeCursor;
                //  And increment the free cursor count.
                nNextFreeCursor++;
            }

            //Make an new connection with the DB.
            //SQLHDBC hConn = newConn();
            SQLHDBC hConn = Ssutil.NewConn();

            //Get a statement handle.
            //sqlRet = SQLAllocHandle(SQL_HANDLE_STMT, hConn, &hStmt);
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            //...Log2.v("\r\nDynAntenna.FtSelectAntenna(): [1] sqlRet = " + sqlRet);

            //Set the statement's attributes.
            //sqlRet = SQLSetStmtAttr(hStmt, SQL_ATTR_CURSOR_TYPE, (SQLPOINTER)SQL_CURSOR_DYNAMIC, 0);
            sqlRet = ODBC.SQLSetStmtAttr(hStmt, ODBC.SQL_ATTR_CURSOR_TYPE, (SQLPOINTER)ODBC.SQL_CURSOR_DYNAMIC, 0);
            //...Log2.v("\r\nDynAntenna.FtSelectAntenna(): [2] sqlRet = " + sqlRet);
            //sqlRet = SQLSetStmtAttr(hStmt, SQL_ATTR_CONCURRENCY, (SQLPOINTER)SQL_CONCUR_VALUES, 0);
            sqlRet = ODBC.SQLSetStmtAttr(hStmt, ODBC.SQL_ATTR_CONCURRENCY, (SQLPOINTER)ODBC.SQL_CONCUR_VALUES, 0);
            //...Log2.v("\r\nDynAntenna.FtSelectAntenna(): [3] sqlRet = " + sqlRet);

            sqlRet = ODBC.SQLExecDirect(hStmt, stmt_buf, ODBC.SQL_NTS);
            //...Log2.v("\r\nDynAntenna.FtSelectAntenna(): [4] sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "Could not Select Antenna for criteria: " + searchCriteria + ".");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.ODBC_EXECUTE_FAILED;
            }

            //	Populate the cursor handle structure
            cursors[curHandle].hConn = hConn;
            cursors[curHandle].hStmt = hStmt;
            cursors[curHandle].tableName = table;
            cursors[curHandle].cursorOpen = ODBC.IsOK(sqlRet);
            cursors[curHandle].pastLastRow = false;
            cursors[curHandle].cCurrentCall1 = "";
            cursors[curHandle].sqlQuery = stmt_buf;

            //...Log2.v("\n\nDynAntenna.FtSelectAntenna(): Exit");
            return curHandle;
        }

        /// <summary>
        /// Inserts an antenna record into the database using column values prescribed 
        /// by the fields of the FtAnte object.
        /// </summary>
        /// <param name="curHandle"> - index of the FtCursor object that encapsulates the details of the record insertion.</param>
        /// <param name="pAnte"> - a FtAnte object.</param>
        /// <param name="nullInd"> - array of ODBC nullInds for pAnte</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - insertion attempt was successful.</para>
        /// <para>-   ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
        /// <para>-   ErrorMessages.DYN_PAST_LAST_ROW - cursor is past the last row.</para>
        /// <para>-   Constant.FAILURE                - deletion attempt failed - ODBC diagnostic information will be written to output.</para>
        /// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int FtInsertAntenna(int curHandle, FtAnte pAnte, SQLLEN[] nullInd)
        {
            //...Log2.v("\n\nDynAntenna.FtInsertAntenna(): Entry");
            //...Log2.v("\nDynAntenna.FtInsertAntenna(): Inserting FtAnte with key:    {0}", pAnte.KeysToStringTerse());

            //char update_buf[STMT_BUF_SZ];
            string update_buf;

            //SQLRETURN sqlRet = 0;
            SQLHANDLE hInsert;

            SQLHANDLE hConn = Ssutil.NewConn();
            if (hConn == SQLHANDLE.Zero)
            {
                Application.Exit("\r\nDynAntenna.FtInsertAntenna(): hConn = Ssutil.NewConn() FAILED");
            }

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hInsert);

            if (!cursors[curHandle].cursorOpen)
            {
                /* Cursor isn't opened yet */
                Ssutil.DisConnStmt(hConn, hInsert);
                return (Error.DYN_CUR_NOT_OPEN);
            }

            if (cursors[curHandle].pastLastRow)
            {
                /* Cursor is past the last row */
                Ssutil.DisConnStmt(hConn, hInsert);
                return (Error.DYN_PAST_LAST_ROW);
            }

            //Create the SQL insert statement.
            update_buf = FtAnte.BuildSqlInsertString(cursors[curHandle].tableName);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            //into global memory with an SQLPOINTER pointer assigned to each one. FtAnte provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = pAnte.CopyToArrayOfSQLPOINTERs();

            // Bind the parameters.
            try
            {
                //This integer is used to enumerate the binding sequence; ODBC definition is that first binding is N = 1;
                int colNum;

                //     dbBindString(hUpdate, 1, "cmd", pAnte->cmd, sizeof(pAnte->cmd), nullInd + FT_ANTE_CMD);
                colNum = 1;
                Ssutil.DbBindStringInput(hInsert, colNum, "cmd", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.cmd.Length, nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 2, "recstat", pAnte->recstat, sizeof(pAnte->recstat), nullInd + FT_ANTE_RECSTAT);
                colNum = 2;
                Ssutil.DbBindStringInput(hInsert, colNum, "recstat", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.recstat.Length, nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 3, "call1", pAnte->call1, sizeof(pAnte->call1), nullInd + FT_ANTE_CALL1);
                colNum = 3;
                Ssutil.DbBindStringInput(hInsert, colNum, "call1", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.call1.Length, nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 4, "call2", pAnte->call2, sizeof(pAnte->call2), nullInd + FT_ANTE_CALL2);
                colNum = 4;
                Ssutil.DbBindStringInput(hInsert, colNum, "call2", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.call2.Length, nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 5, "bndcde", pAnte->bndcde, sizeof(pAnte->bndcde), nullInd + FT_ANTE_BNDCDE);
                colNum = 5;
                Ssutil.DbBindStringInput(hInsert, colNum, "bndcde", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.bndcde.Length, nullIndPtr[colNum - 1]);

                //    dbBindSShort(hInsert, 6, "anum", &pAnte->anum, nullInd + FT_ANTE_ANUM);
                colNum = 6;
                Ssutil.DbBindShortInput(hInsert, colNum, "anum", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 7, "ause", pAnte->ause, sizeof(pAnte->ause), nullInd + FT_ANTE_AUSE);
                colNum = 7;
                Ssutil.DbBindStringInput(hInsert, colNum, "ause", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.ause.Length, nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 8, "acode", pAnte->acode, sizeof(pAnte->acode), nullInd + FT_ANTE_ACODE);
                colNum = 8;
                Ssutil.DbBindStringInput(hInsert, colNum, "acode", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.acode.Length, nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 9, "aht", &pAnte->aht, nullInd + FT_ANTE_AHT);
                colNum = 9;
                Ssutil.DbBindFloatInput(hInsert, colNum, "aht", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 10, "azmth", &pAnte->azmth, nullInd + FT_ANTE_AZMTH);
                colNum = 10;
                Ssutil.DbBindFloatInput(hInsert, colNum, "azmth", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 11, "elvtn", &pAnte->elvtn, nullInd + FT_ANTE_ELVTN);
                colNum = 11;
                Ssutil.DbBindFloatInput(hInsert, colNum, "elvtn", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 12, "dist", &pAnte->dist, nullInd + FT_ANTE_DIST);
                colNum = 12;
                Ssutil.DbBindFloatInput(hInsert, colNum, "dist", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 13, "offazm", pAnte->offazm, sizeof(pAnte->offazm), nullInd + FT_ANTE_OFFAZM);
                colNum = 13;
                Ssutil.DbBindStringInput(hInsert, colNum, "offazm", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.offazm.Length, nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 14, "tazmth", &pAnte->tazmth, nullInd + FT_ANTE_TAZMTH);
                colNum = 14;
                Ssutil.DbBindFloatInput(hInsert, colNum, "tazmth", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 15, "telvtn", &pAnte->telvtn, nullInd + FT_ANTE_TELVTN);
                colNum = 15;
                Ssutil.DbBindFloatInput(hInsert, colNum, "telvtn", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 16, "tgain", &pAnte->tgain, nullInd + FT_ANTE_TGAIN);
                colNum = 16;
                Ssutil.DbBindFloatInput(hInsert, colNum, "tgain", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 17, "txfdlnth", pAnte->txfdlnth, sizeof(pAnte->txfdlnth), nullInd + FT_ANTE_TXFDLNTH);
                colNum = 17;
                Ssutil.DbBindStringInput(hInsert, colNum, "txfdlnth", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.txfdlnth.Length, nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 18, "txfdlnlh", &pAnte->txfdlnlh, nullInd + FT_ANTE_TXFDLNLH);
                colNum = 18;
                Ssutil.DbBindFloatInput(hInsert, colNum, "txfdlnlh", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 19, "txfdlntv", pAnte->txfdlntv, sizeof(pAnte->txfdlntv), nullInd + FT_ANTE_TXFDLNTV);
                colNum = 19;
                Ssutil.DbBindStringInput(hInsert, colNum, "txfdlntv", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.txfdlntv.Length, nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 20, "txfdlnlv", &pAnte->txfdlnlv, nullInd + FT_ANTE_TXFDLNLV);
                colNum = 20;
                Ssutil.DbBindFloatInput(hInsert, colNum, "txfdlnlv", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 21, "rxfdlnth", pAnte->rxfdlnth, sizeof(pAnte->rxfdlnth), nullInd + FT_ANTE_RXFDLNTH);
                colNum = 21;
                Ssutil.DbBindStringInput(hInsert, colNum, "rxfdlnth", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.rxfdlnth.Length, nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 22, "rxfdlnlh", &pAnte->rxfdlnlh, nullInd + FT_ANTE_RXFDLNLH);
                colNum = 22;
                Ssutil.DbBindFloatInput(hInsert, colNum, "rxfdlnlh", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 23, "rxfdlntv", pAnte->rxfdlntv, sizeof(pAnte->rxfdlntv), nullInd + FT_ANTE_RXFDLNTV);
                colNum = 23;
                Ssutil.DbBindStringInput(hInsert, colNum, "rxfdlntv", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.rxfdlntv.Length, nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 24, "rxfdlnlv", &pAnte->rxfdlnlv, nullInd + FT_ANTE_RXFDLNLV);
                colNum = 24;
                Ssutil.DbBindFloatInput(hInsert, colNum, "rxfdlnlv", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 25, "txpadpam", &pAnte->txpadpam, nullInd + FT_ANTE_TXPADPAM);
                colNum = 25;
                Ssutil.DbBindFloatInput(hInsert, colNum, "txpadpam", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 26, "rxpadlna", &pAnte->rxpadlna, nullInd + FT_ANTE_RXPADLNA);
                colNum = 26;
                Ssutil.DbBindFloatInput(hInsert, colNum, "rxpadlna", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 27, "txcompl", &pAnte->txcompl, nullInd + FT_ANTE_TXCOMPL);
                colNum = 27;
                Ssutil.DbBindFloatInput(hInsert, colNum, "txcompl", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 28, "rxcompl", &pAnte->rxcompl, nullInd + FT_ANTE_RXCOMPL);
                colNum = 28;
                Ssutil.DbBindFloatInput(hInsert, colNum, "rxcompl", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 29, "obsloss", &pAnte->obsloss, nullInd + FT_ANTE_OBSLOSS);
                colNum = 29;
                Ssutil.DbBindFloatInput(hInsert, colNum, "obsloss", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindFloat(hInsert, 30, "kvalue", &pAnte->kvalue, nullInd + FT_ANTE_KVALUE);
                colNum = 30;
                Ssutil.DbBindFloatInput(hInsert, colNum, "kvalue", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindSShort(hInsert, 31, "atwrno", &pAnte->atwrno, nullInd + FT_ANTE_ATWRNO);
                colNum = 31;
                Ssutil.DbBindShortInput(hInsert, colNum, "atwrno", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 32, "nota", pAnte->nota, sizeof(pAnte->nota), nullInd + FT_ANTE_NOTA);
                colNum = 32;
                Ssutil.DbBindStringInput(hInsert, colNum, "nota", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.nota.Length, nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 33, "apoint", pAnte->apoint, sizeof(pAnte->apoint), nullInd + FT_ANTE_APOINT);
                colNum = 33;
                Ssutil.DbBindStringInput(hInsert, colNum, "apoint", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.apoint.Length, nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 34, "sdate", pAnte->sdate, sizeof(pAnte->sdate), nullInd + FT_ANTE_SDATE);
                colNum = 34;
                Ssutil.DbBindStringInput(hInsert, colNum, "sdate", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.sdate.Length, nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 35, "licence", pAnte->licence, sizeof(pAnte->licence), nullInd + FT_ANTE_LICENCE);
                colNum = 35;
                Ssutil.DbBindStringInput(hInsert, colNum, "licence", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.licence.Length, nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 36, "mdate", pAnte->mdate, sizeof(pAnte->mdate), nullInd + FT_ANTE_MDATE);
                colNum = 36;
                Ssutil.DbBindStringInput(hInsert, colNum, "mdate", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.mdate.Length, nullIndPtr[colNum - 1]);

                //    dbBindString(hInsert, 37, "mtime", pAnte->mtime, sizeof(pAnte->mtime), nullInd + FT_ANTE_MTIME);
                colNum = 37;
                Ssutil.DbBindStringInput(hInsert, colNum, "mtime", parameterValuePtr[colNum - 1], (SQLULEN)pAnte.mtime.Length, nullIndPtr[colNum - 1]);
            }
            catch (Exception e)
            {
                Ssutil.DbGetDiagStmt(hInsert, "ftInsertAntenna01 -- Error binding parameters for site on field: " + e.Message);
                Ssutil.DisConnStmt(hConn, hInsert);
                return Constant.FAILURE;
            }

            //...Log2.v("\r\nDynAntenna.FtInsertAntenna(): SQLExecDirect():\r\n" + update_buf);
            SQLRETURN sqlRet;
            sqlRet = ODBC.SQLExecDirect(hInsert, update_buf, update_buf.Length);

            //...Log2.v("\r\nDynAntenna.FtInsertAntenna(): SQLExecDirect(): sqlRet = " + sqlRet);

            if (!ODBC.IsOK(sqlRet))
            {
                string str = "ftInsertAntenna02 -- Error writing Antenna: " + pAnte.call1 + " " + pAnte.call2 + " " + pAnte.anum;
                Ssutil.DbGetDiagStmt(hInsert, str);
                Log2.e("\n\nDynAntenna.FtInsertAntenna(): ERROR: sqlRet = {0} for keys = {1}", sqlRet, pAnte.KeysToStringTerse());
                Log2.e("\n" + ODBC.GetDiagnostics(hInsert, update_buf));
                Ssutil.DisConnStmt(hConn, hInsert);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            //This method call releases hInsert, disconnects from the DB and then releases hConn.
            Ssutil.DisConnStmt(hConn, hInsert);

            //Application.Exit("DynAntenna.FtInsertAntenna(): forced termination.");

            //...Log2.v("\n\nDynAntenna.FtInsertAntenna(): Exit");
            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method returns a fully-populated FtAnte object and its associated array of nullInds
        /// corresponding to a single record fetched from the User table ft_XXX_ante where XXX is 
        /// the PDF name; the (FtAnte) record is uniquely identified by its key values (call1, 
        /// call2, bndcde, anum).
        /// </summary>
        /// <param name="pdfName"></param>
        /// <param name="call1"></param>
        /// <param name="call2"></param>
        /// <param name="bndcde"></param>
        /// <param name="anum"></param>
        /// <param name="ftAnte"></param>
        /// <param name="nullInds"></param>
        /// <param name="found"></param>
        /// <returns></returns>
        public static int FetchFtAnteByKey(string pdfName, string call1, string call2, string bndcde, short anum,
                                           out FtAnte ftAnte, out SQLLEN[] nullInds, out bool found)
        {
            // 'out' requirements.
            ftAnte = null;
            nullInds = null;
            found = false;

            int retVal = Constant.SUCCESS;
            string tableName;
            int cursorID;
            int rc;
            string whereClause;

            // Get the complete SQL Table name from the PDF 'short' name.
            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);

            // Form the SQL SELECT where clause.
            whereClause = String.Format("call1='{0}' AND call2='{1}' AND bndcde='{2}' AND anum='{3}'",
                                         call1, call2, bndcde, anum);

            // SELECT the ftAnte by its key.
            cursorID = DynAntenna.FtSelectAntenna(tableName, whereClause, "");

            // If the SELECT worked then attempt to fetch the ftSite.
            if (cursorID < 0)
            {
                retVal = Constant.FAILURE;
            }
            else
            {
                // Constant.SUCCESS                - fetch attempt was successful.</para>
                // Constant.FAILURE                - fetch attempt failed.</para>
                // ErrorMessages.DYN_CUR_NOT_OPEN  - FtCursor field cursorOpen is set to false.</para>
                // ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
                // ErrorMessages.ODBC_GET_FAILED    - call to ODBC.SQLGetData() threw an exception. </para>
                rc = DynAntenna.FtFetchAntenna(cursorID, out ftAnte, out nullInds);

                switch (rc)
                {
                    case Constant.SUCCESS:
                        retVal = Constant.SUCCESS;
                        found = true;
                        break;
                    case ODBC.SQL_NO_DATA:
                        found = false;
                        retVal = Constant.SUCCESS;
                        break;
                    default:
                        retVal = Constant.FAILURE;
                        break;
                }
            }

            DynAntenna.FtCloseAntenna(cursorID);

            return retVal;
        }

        /// <summary>
        /// This method returns true if a record exists in a table that has a prescribed 
        /// key: call1, call2, bndcde and anum as given by the ftAnte object; the table name 
        /// is prescribed via the cursor handle.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="ftAnte"></param>
        /// <returns></returns>
        public static bool RecordWithKeyExists(int handle, FtAnte ftAnte)
        {
            string whereClause = String.Format(" call1 = '{0}' AND call2 = '{1}' AND bndcde = '{2}' AND anum = '{3}' ",
                                                ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum);

            // These columns comprise the 'key' for a ft_XXX_ante table record and so there
            // can only be 0 or 1 records.
            return (Ssutil.DbCountRows(cursors[handle].tableName, whereClause) == 1);
        }


    }
}

```
