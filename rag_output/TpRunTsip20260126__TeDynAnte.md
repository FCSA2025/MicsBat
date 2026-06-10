# Documented File: TeDynAnte.cs
**Repository Path:** `TpRunTsip20260126\TeDynAnte.cs`
**Primary Layer:** `TpRunTsip20260126`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TpRunTsip
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using System.Runtime.InteropServices;
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
    /// Provides methods to prepare and insert 
    /// records into the database table <b>&lt;userID&gt;.te_&lt;tableName&gt;_&lt;runID&gt;_ante</b>
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item> The methods in this class are used to perform operations on the
    /// te ANTE table. The actual ANTE table name is a variable, prescribed by the caller.</item>
    /// <item>The supported operations are: TePrepareAnte, TeInsertAnte and TeAnteClose.</item>
    /// <item>The caller must first call the method TtPrepareAnte() that creates a cursor 
    /// for use with a subsequent call to TeInsertAnte().</item>
    /// <item>The method closeSite must be called to release internal cursor resources (a cursor 
    /// encapsulates an ODBC connection and statement handle et. al.).</item>
    /// </list>
    /// </remarks>
    public class TeDynAnte
    {
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int tePrepareAnte([In] string table);

        //--------------------------------------------------------------------

        public static SQLHANDLE hTEAStmt;
        public static SQLHDBC hTEAConn = SQLHDBC.Zero;
        public static string cAnteTable;

        /// <summary>
        /// This method calls ODBC.SQLPrepare() to prepare a table insert on an internal (private) statement 
        /// handle that can then be used in a subsequent call to ODBC.SQLExecute to
        /// actually perform the SQL record insertion.
        /// </summary>
        /// <param name="table"> - full name of the DB table into which the record is to be inserted.</param>
        /// <returns></returns>
        public static int TePrepareAnte(string table)
        {
            string insert_buf;

            SQLRETURN sqlRet;

            cAnteTable = table;

            // For a hybrid build, we need to set the static variable cAnteTable
            // in native file teDynAnte.cpp.
#if PINVOKE
            tePrepareAnte(table);
#endif

            insert_buf = String.Format("insert into {0} (interferer, terrcall1, terrcall2, terrbndcde, terranum, earthlocation, earthcall1, earthband, terracode, earthacode, satname, satoper, satlongit, txpre, txtro, rxpre, rxtro, sarc1, sarc2, intause, mode1, mode2, etreport, tereport, etsubcaseno, tesubcaseno, esazim, eselev, teelev, etelev, tuelev, utelev, euelev, ediscang, tdiscang, adisc_set, adisc_ute, terrht, earthht, tvazim, evazim, tvelev, evelev, tvdistes, tvdisttu, evdistes, evdisttu, angleutv, anglesev, tsoffaxis, tstrueaz, tstrueel, angleute, angleuta, angleeta, angleatv, adisc_atv, terragain, terramodel, terraxref, earthagain, earthamodel, earthaxref, processed) values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", table);

            hTEAConn = Ssutil.NewConn();

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hTEAConn, out hTEAStmt);

            sqlRet = ODBC.SQLPrepare(hTEAStmt, insert_buf, insert_buf.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTeDynAnte.TePrepareAnte(): ERROR: SQLPrepare() failed.");

                Ssutil.DbGetDiagStmt(hTEAStmt, "tePrepareAnte01: Could not prepare TE antenna:-");

                return Error.ODBC_PREPARE_FAILED;
            }

            return Constant.SUCCESS;
        }

        /// <summary>
        /// This method closes the internal (private) cursor object by freeing the ODBC 
        /// connection and statement handles. 
        /// </summary>
        /// <param name=""></param> 
        public static int TeCloseAnte()
        {
            if (hTEAConn != null)
            {
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hTEAStmt);

                hTEAStmt = SQLHANDLE.Zero;

                Ssutil.DisConn(hTEAConn);

                hTEAConn = SQLHDBC.Zero;
            }
            return 0;
        }

        /// <summary>
        /// This method calls ODBC.SQLExecute() to perform insertion of a record
        /// into a DB table by using the internal (private) ODBC statement handle
        /// that was prepared on by a previous call to ODBC.SQLPrepare().
        /// </summary>
        /// <param name="teAnte"> - TeAnte object (record) to be inserted into the DB table.</param>
        /// <param name="nullInd"> - array of ODBC nullInds associated with teAnte.</param>
        /// <returns></returns>
        public static int TeInsertAnte(TeAnte teAnte, SQLLEN[] nullInd)
        {
            //&&Console.Error.Write("\nteDynAnte.teInsertAnte(): insertStruct.esazim = {0}", insertStruct.esazim);
            SQLRETURN sqlRet;

            if (hTEAConn == null)
            {
                /*  Reprepare because of commits */
                TePrepareAnte(cAnteTable);
            }


            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This neccessitates copying the values of the nullInd
            //array elements into global memory with an SQLLENPTR pointer assigned to each one.
            SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

            //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
            //to contain the values to bind to. This necessitates copying the 'column' values of pAnte
            //into global memory with an SQLPOINTER pointer assigned to each one. FtAnte provides
            //a convenience method that does exactly this.
            SQLPOINTER[] parameterValuePtr = teAnte.CopyToArrayOfSQLPOINTERs();

            try
            {
                // Initialize auto-indexing.
                Ssutil.DbStartBinds();

                Ssutil.DbBindStringInput(hTEAStmt, 0, "interferer", parameterValuePtr[TeAnte.INTERFERER], TeAnte.INTERFERER_SZ, nullIndPtr[TeAnte.INTERFERER]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "terrcall1", parameterValuePtr[TeAnte.TERRCALL1], TeAnte.TERRCALL1_SZ, nullIndPtr[TeAnte.TERRCALL1]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "terrcall2", parameterValuePtr[TeAnte.TERRCALL2], TeAnte.TERRCALL2_SZ, nullIndPtr[TeAnte.TERRCALL2]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "terrbndcde", parameterValuePtr[TeAnte.TERRBNDCDE], TeAnte.TERRBNDCDE_SZ, nullIndPtr[TeAnte.TERRBNDCDE]);
                Ssutil.DbBindShortInput(hTEAStmt, 0, "terranum", parameterValuePtr[TeAnte.TERRANUM], nullIndPtr[TeAnte.TERRANUM]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "earthlocation", parameterValuePtr[TeAnte.EARTHLOCATION], TeAnte.EARTHLOCATION_SZ, nullIndPtr[TeAnte.EARTHLOCATION]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "earthcall1", parameterValuePtr[TeAnte.EARTHCALL1], TeAnte.EARTHCALL1_SZ, nullIndPtr[TeAnte.EARTHCALL1]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "earthband", parameterValuePtr[TeAnte.EARTHBAND], TeAnte.EARTHBAND_SZ, nullIndPtr[TeAnte.EARTHBAND]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "terracode", parameterValuePtr[TeAnte.TERRACODE], TeAnte.TERRACODE_SZ, nullIndPtr[TeAnte.TERRACODE]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "earthacode", parameterValuePtr[TeAnte.EARTHACODE], TeAnte.EARTHACODE_SZ, nullIndPtr[TeAnte.EARTHACODE]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "satname", parameterValuePtr[TeAnte.SATNAME], TeAnte.SATNAME_SZ, nullIndPtr[TeAnte.SATNAME]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "satoper", parameterValuePtr[TeAnte.SATOPER], TeAnte.SATOPER_SZ, nullIndPtr[TeAnte.SATOPER]);
                Ssutil.DbBindIntInput(hTEAStmt, 0, "satlongit", parameterValuePtr[TeAnte.SATLONGIT], nullIndPtr[TeAnte.SATLONGIT]);
                Ssutil.DbBindFloatInput(hTEAStmt, 0, "txpre", parameterValuePtr[TeAnte.TXPRE], nullIndPtr[TeAnte.TXPRE]);
                Ssutil.DbBindFloatInput(hTEAStmt, 0, "txtro", parameterValuePtr[TeAnte.TXTRO], nullIndPtr[TeAnte.TXTRO]);
                Ssutil.DbBindFloatInput(hTEAStmt, 0, "rxpre", parameterValuePtr[TeAnte.RXPRE], nullIndPtr[TeAnte.RXPRE]);
                Ssutil.DbBindFloatInput(hTEAStmt, 0, "rxtro", parameterValuePtr[TeAnte.RXTRO], nullIndPtr[TeAnte.RXTRO]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "sarc1", parameterValuePtr[TeAnte.SARC1], nullIndPtr[TeAnte.SARC1]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "sarc2", parameterValuePtr[TeAnte.SARC2], nullIndPtr[TeAnte.SARC2]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "intause", parameterValuePtr[TeAnte.INTAUSE], TeAnte.INTAUSE_SZ, nullIndPtr[TeAnte.INTAUSE]);
                Ssutil.DbBindShortInput(hTEAStmt, 0, "mode1", parameterValuePtr[TeAnte.MODE1], nullIndPtr[TeAnte.MODE1]);
                Ssutil.DbBindShortInput(hTEAStmt, 0, "mode2", parameterValuePtr[TeAnte.MODE2], nullIndPtr[TeAnte.MODE2]);
                Ssutil.DbBindShortInput(hTEAStmt, 0, "etreport", parameterValuePtr[TeAnte.ETREPORT], nullIndPtr[TeAnte.ETREPORT]);
                Ssutil.DbBindShortInput(hTEAStmt, 0, "tereport", parameterValuePtr[TeAnte.TEREPORT], nullIndPtr[TeAnte.TEREPORT]);
                Ssutil.DbBindIntInput(hTEAStmt, 0, "etsubcaseno", parameterValuePtr[TeAnte.ETSUBCASENO], nullIndPtr[TeAnte.ETSUBCASENO]);
                Ssutil.DbBindIntInput(hTEAStmt, 0, "tesubcaseno", parameterValuePtr[TeAnte.TESUBCASENO], nullIndPtr[TeAnte.TESUBCASENO]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "esazim", parameterValuePtr[TeAnte.ESAZIM], nullIndPtr[TeAnte.ESAZIM]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "eselev", parameterValuePtr[TeAnte.ESELEV], nullIndPtr[TeAnte.ESELEV]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "teelev", parameterValuePtr[TeAnte.TEELEV], nullIndPtr[TeAnte.TEELEV]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "etelev", parameterValuePtr[TeAnte.ETELEV], nullIndPtr[TeAnte.ETELEV]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "tuelev", parameterValuePtr[TeAnte.TUELEV], nullIndPtr[TeAnte.TUELEV]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "utelev", parameterValuePtr[TeAnte.UTELEV], nullIndPtr[TeAnte.UTELEV]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "euelev", parameterValuePtr[TeAnte.EUELEV], nullIndPtr[TeAnte.EUELEV]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "ediscang", parameterValuePtr[TeAnte.EDISCANG], nullIndPtr[TeAnte.EDISCANG]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "tdiscang", parameterValuePtr[TeAnte.TDISCANG], nullIndPtr[TeAnte.TDISCANG]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "adisc_set", parameterValuePtr[TeAnte.ADISC_SET], nullIndPtr[TeAnte.ADISC_SET]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "adisc_ute", parameterValuePtr[TeAnte.ADISC_UTE], nullIndPtr[TeAnte.ADISC_UTE]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "terrht", parameterValuePtr[TeAnte.TERRHT], nullIndPtr[TeAnte.TERRHT]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "earthht", parameterValuePtr[TeAnte.EARTHHT], nullIndPtr[TeAnte.EARTHHT]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "tvazim", parameterValuePtr[TeAnte.TVAZIM], nullIndPtr[TeAnte.TVAZIM]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "evazim", parameterValuePtr[TeAnte.EVAZIM], nullIndPtr[TeAnte.EVAZIM]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "tvelev", parameterValuePtr[TeAnte.TVELEV], nullIndPtr[TeAnte.TVELEV]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "evelev", parameterValuePtr[TeAnte.EVELEV], nullIndPtr[TeAnte.EVELEV]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "tvdistes", parameterValuePtr[TeAnte.TVDISTES], nullIndPtr[TeAnte.TVDISTES]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "tvdisttu", parameterValuePtr[TeAnte.TVDISTTU], nullIndPtr[TeAnte.TVDISTTU]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "evdistes", parameterValuePtr[TeAnte.EVDISTES], nullIndPtr[TeAnte.EVDISTES]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "evdisttu", parameterValuePtr[TeAnte.EVDISTTU], nullIndPtr[TeAnte.EVDISTTU]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "angleutv", parameterValuePtr[TeAnte.ANGLEUTV], nullIndPtr[TeAnte.ANGLEUTV]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "anglesev", parameterValuePtr[TeAnte.ANGLESEV], nullIndPtr[TeAnte.ANGLESEV]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "tsoffaxis", parameterValuePtr[TeAnte.TSOFFAXIS], TeAnte.TSOFFAXIS_SZ, nullIndPtr[TeAnte.TSOFFAXIS]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "tstrueaz", parameterValuePtr[TeAnte.TSTRUEAZ], nullIndPtr[TeAnte.TSTRUEAZ]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "tstrueel", parameterValuePtr[TeAnte.TSTRUEEL], nullIndPtr[TeAnte.TSTRUEEL]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "angleute", parameterValuePtr[TeAnte.ANGLEUTE], nullIndPtr[TeAnte.ANGLEUTE]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "angleuta", parameterValuePtr[TeAnte.ANGLEUTA], nullIndPtr[TeAnte.ANGLEUTA]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "angleeta", parameterValuePtr[TeAnte.ANGLEETA], nullIndPtr[TeAnte.ANGLEETA]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "angleatv", parameterValuePtr[TeAnte.ANGLEATV], nullIndPtr[TeAnte.ANGLEATV]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "adisc_atv", parameterValuePtr[TeAnte.ADISC_ATV], nullIndPtr[TeAnte.ADISC_ATV]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "terragain", parameterValuePtr[TeAnte.TERRAGAIN], nullIndPtr[TeAnte.TERRAGAIN]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "terramodel", parameterValuePtr[TeAnte.TERRAMODEL], TeAnte.TERRAMODEL_SZ, nullIndPtr[TeAnte.TERRAMODEL]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "terraxref", parameterValuePtr[TeAnte.TERRAXREF], TeAnte.TERRAXREF_SZ, nullIndPtr[TeAnte.TERRAXREF]);
                Ssutil.DbBindDoubleInput(hTEAStmt, 0, "earthagain", parameterValuePtr[TeAnte.EARTHAGAIN], nullIndPtr[TeAnte.EARTHAGAIN]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "earthamodel", parameterValuePtr[TeAnte.EARTHAMODEL], TeAnte.EARTHAMODEL_SZ, nullIndPtr[TeAnte.EARTHAMODEL]);
                Ssutil.DbBindStringInput(hTEAStmt, 0, "earthaxref", parameterValuePtr[TeAnte.EARTHAXREF], TeAnte.EARTHAXREF_SZ, nullIndPtr[TeAnte.EARTHAXREF]);
                Ssutil.DbBindIntInput(hTEAStmt, 0, "processed", parameterValuePtr[TeAnte.PROCESSED], nullIndPtr[TeAnte.PROCESSED]);
            }
            catch (Exception e)
            {
                Log2.e("\nTeDynAnte.TeInsertAnte(): ERROR: ODBC Binding failed: " + e.Message);
                Ssutil.DbGetDiagStmt(hTEAStmt, "teInsertAnte01: Error binding field " + e.Message);
                return Error.ODBC_BINDING_FAILED;
            }

            sqlRet = ODBC.SQLExecute(hTEAStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTeDynAnte.TeInsertAnte(): ERROR: SQLExecute() failed: ");
                Ssutil.DbGetDiagStmt(hTEAStmt, "teInsertAnte02: Execution error for TE Antenna:");
                return Error.ODBC_EXECUTE_FAILED;
            }

            return (Constant.SUCCESS);
        }



    }
}

```
