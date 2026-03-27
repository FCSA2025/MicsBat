using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeValidate
{
    using _Configuration;
    using _DataStructures;
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
    /// This class provides methods that fetch records from the MDB site, antenna, azimuth and channel
    /// tables and 'import' them into the PDF data set if a record with the same key does not already 
    /// exist there.
    /// </summary>
    public class ValImport
    {
        private static string MDBWhereClause;

        /// <summary>
        /// This method creates a string that can be used as the 'whereClause' in an
        /// SQL SELECT query; the 'whereClause' searches for the prescribed location, callsign and channel ID.
        /// </summary>
        /// <param name="whereClause">The form clause.</param>
        /// <param name="location">The location.</param>
        /// <param name="call1">The call1.</param>
        /// <param name="chid">The chid.</param>
        public static void FeFormWhereClause(out string whereClause,  // area to put the clause 
                                                 string location,   // valued requested for call sign 1 
                                                 string call1,      // valued requested for call sign 2 
                                                 string chid)       // valued requested for channel ID 
        {
            // 'out' requirement.
            whereClause = "";

            string tempClause = "";
            bool isMultiple = false;     // query on multiple fields or not 

            if (!String.IsNullOrWhiteSpace(location))
            {
                tempClause = String.Format(" location = '{0}'", location);
                isMultiple = true;
                whereClause += tempClause;
            }

            if (!String.IsNullOrWhiteSpace(call1))
            {
                tempClause = String.Format("call1 = '{0}' ", call1);
                if (isMultiple)
                {
                    whereClause += " and ";
                }
                isMultiple = true;
                whereClause += tempClause;
            }

            if (!String.IsNullOrWhiteSpace(chid))
            {
                tempClause = String.Format("chid = '{0}' ", chid);
                if (isMultiple)
                {
                    whereClause += " and ";
                }
                isMultiple = true;
                whereClause += tempClause;
            }

            if (!isMultiple)
            {
                whereClause = " location like '%' ";
            }
        }

        /// <summary>
        /// This methods checks whether prescribed antenna records (selected by the 'whereClause') exist in the PDF's _ante table; 
        /// if an individual antenna record is not in the PDF's _ante table then it is copied in from the MDB's me_ante table.         
        /// </summary>
        /// <remarks>
        /// The import module will test to see if the requested antenna record already 
        /// exists in the PDF, if it is not succesful in finding the record it will 
        /// then attempt to copy it from the MDB. If that is unsuccessful then it will 
        /// produce an error message.  
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="whereClause"> - 'where' clause to use in an SQL SELECT query that selects the records.</param>
        /// <param name="cmd"> - MDB operation command code to put in the records.</param>
        public static void FeImportAntenna(string pdfName,    // pdf display name 
                                            string whereClause,  // 'where' clause to use in an SQL SELECT query 
                                            string cmd)        // command code to put in the record 
        {
            MeAnte meAnte;
            SQLLEN[] nArrayMDB;

            int aID1;
            int rc;
            string tableName;
            string dynClause;

            FeAnte tempAnte = new FeAnte();
            SQLLEN[] nArrayFW = NullHelper.CreateArrayOfNullInd(FeAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            int nHandle;
            int nRet;

            MDBWhereClause = whereClause;

            GenUtil.UtCvtName(Constant.FE_ANTE, pdfName, out tableName);

            //...Log2.v(String.Format("\n\nValImport.FeImportAntenna(): table = {0}; condition = {1}", tableName, whereClause));

            // open ante cursor and fetch first record 
            nHandle = DynMeAnte.MeSelectAnte(whereClause, "call1");
            if (nHandle < 0)
            {
                Log2.e("\nValImport.FeImportAntenna(): call to MeSelectAnte() failed, returned: " + nHandle);
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                return;
            }

            nRet = DynMeAnte.MeFetchAnte(nHandle, out meAnte, out nArrayMDB);

            while (nRet == 0)
            {
                // verify that it is not in the pdf 
                dynClause = String.Format("location = '{0}' and call1 = '{1}' ", meAnte.location, meAnte.call1);

                if ((aID1 = DynFeAnte.FeSelectAnte(tableName, dynClause, "")) < 0)
                {
                    Log2.e("\nValImport.FeImportAntenna(): call to FeSelectAnte() failed, returned: " + aID1);
                    Console.Write("IMPORT - could not read antenna information\r\n");
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    rc = Constant.FAILURE;
                }
                else
                {
                    rc = DynFeAnte.FeFetchAnte(aID1, out tempAnte, out nArrayFW);
                }

                if (rc != Constant.SUCCESS)
                {
                    FeValCopy.FeCopyAnte(ref tempAnte, meAnte, ref nArrayFW, nArrayMDB);

                    tempAnte.cmd = cmd;
                    nArrayFW[FeAnte.RECSTAT] = Constant.DB_NOT_NULL;

                    tempAnte.recstat = "C";
                    nArrayFW[FeAnte.CMD] = Constant.DB_NOT_NULL;

                    rc = DynFeAnte.FeInsertAnte(aID1, tempAnte, nArrayFW);
                    if (rc != Constant.SUCCESS)
                    {
                        Log2.e("\nValImport.FeImportAntenna(): call to FeInsertAnte() failed, returned: " + rc);
                        Console.Write("IMPORT - could not insert antenna\r\n");
                        ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    }
                }
                DynFeAnte.FeCloseAnte(aID1);

                nRet = DynMeAnte.MeFetchAnte(nHandle, out meAnte, out nArrayMDB);
            }

            if (nRet != Constant.NOMORERECS)
            {
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            DynMeAnte.MeCloseAnte(nHandle);

            return;
        }

        /// <summary>
        /// This methods checks whether prescribed channel records (selected by the 'whereClause') exist in the PDF's _chan table; 
        /// if an individual channel record is not in the PDF's _chan table then it is copied in from the MDB's me_chan table.  
        /// </summary>
        /// <remarks>
        /// The import module will test to see if the requested channel records already 
        /// exists in the PDF, if it is not succesful in finding a record it will 
        /// then attempt to copy it from the MDB. If that is unsuccessful then it will 
        /// produce an error message.  
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="whereClause"> - 'where' clause to use in an SQL SELECT query that selects the records.</param>
        /// <param name="cmd"> - MDB operation command code to put in the records.</param>
        public static void FeImportChannel(string pdfName,     // pdf display name 
                                            string whereClause,  // 'where' clause to use in an SQL SELECT query 
                                            string cmd)        // command code to put in the record 
        {

            MeChan meChan;
            SQLLEN[] nArrayMDB; //[ME_CHAN_SIZE_];
            SQLLEN[] nArrayFW; //[FeChan.SIZE_];
            int rc;
            int cID1;
            string dynClause;
            string tableName;
            FeChan tempChan;

            int nHandle;
            int nRet;

            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out tableName);

            //...Log2.v(String.Format("\n\nValImport.FeImportChannel(): table = {0}; condition = {1}", tableName, whereClause));

            // Open chan cursor and fetch first record.
            nHandle = DynMeChan.MeSelectChan(whereClause, "call1, chid");
            if (nHandle < 0)
            {
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            nRet = DynMeChan.MeFetchChan(nHandle, out meChan, out nArrayMDB);

            while (nRet == 0)
            {
                // verify that it is not in the pdf 
                FeFormWhereClause(out dynClause, meChan.location, meChan.call1, meChan.chid);

                if ((cID1 = DynFeChan.FeSelectChan(tableName, dynClause, "")) < 0)
                {
                    Console.Write("IMPORT - could not read channel information\r\n");
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    rc = Constant.FAILURE;
                }
                else
                {
                    rc = DynFeChan.FeFetchChan(cID1, out tempChan, out nArrayFW);
                }

                if (rc != Constant.SUCCESS)
                {
                    tempChan = new FeChan();
                    nArrayFW = NullHelper.CreateArrayOfNullInd(FeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                    FeValCopy.FeCopyChan(ref tempChan, meChan, ref nArrayFW, nArrayMDB);

                    tempChan.recstat = "C";
                    tempChan.cmd = cmd;
                    nArrayFW[FeChan.RECSTAT] = Constant.DB_NOT_NULL;
                    nArrayFW[FeChan.CMD] = Constant.DB_NOT_NULL;

                    if (DynFeChan.FeInsertChan(cID1, tempChan, nArrayFW) != Constant.SUCCESS)
                    {
                        Console.Write("IMPORT - could not insert channel \r\n");
                        ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    }
                }

                DynFeChan.FeCloseChan(cID1);

                nRet = DynMeChan.MeFetchChan(nHandle, out meChan, out nArrayMDB);
            }

            if (nRet != Constant.NOMORERECS)
            {
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            DynMeChan.MeCloseChan(nHandle);
        }

        /// <summary>
        /// This methods checks whether prescribed azimuth records (selected by the 'whereClause') exist in the PDF's _azim table; 
        /// if an individual azimuth record is not in the PDF's _azim table then it is copied in from the MDB's me_azim table.  
        /// </summary>
        /// <remarks>
        /// This import method tests to see if the requested azimuth record already 
        /// exists in the PDF, if it is not succesful in finding the record it will 
        /// then attempt to copy it from the MDB. If that is unsuccessful then it will 
        /// produce an error message.  
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="whereClause"> - the 'where' clause to use in the SQL query to select the prescribed azimuth record.</param>
        /// <param name="cmd"> - MDB operation command code to put in the record.</param>
        public static void FeImportAzimuth(string pdfName,     // pdf display name 
                                            string whereClause,  // SQL SELECT 'where' clause 
                                            string cmd)        // command code to put in the record 
        {

            MeAzim meAzim;
            SQLLEN[] nArrayMDB;  //[ME_AZIM_SIZE_];
            SQLLEN[] nArrayFW;   //[FeAzim.SIZE_];
            short rc;
            int cID1;
            string dynClause;
            string tableName;
            FeAzim tempAzim;

            int nHandle;
            int nRet;

            GenUtil.UtCvtName(Constant.FE_AZIM, pdfName, out tableName);

            //...Log2.v(String.Format("\n\nValImport.FeImportAzimuth(): table = {0}; condition = {1}", tableName, whereClause));

            // Open Azim cursor and fetch first record.
            nHandle = DynMeAzim.MeSelectAzim(whereClause, "call1,azim");

            if (nHandle < 0)
            {
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                return;
            }

            nRet = DynMeAzim.MeFetchAzim(nHandle, out meAzim, out nArrayMDB);

            while (nRet == 0)
            {
                // verify that it is not in the pdf 
                dynClause = String.Format("location = '{0}' and call1 = '{1}' and azim = {2}",
                          meAzim.location, meAzim.call1, meAzim.azim);

                if ((cID1 = DynFeAzim.FeSelectAzim(tableName, dynClause, "")) < 0)
                {
                    Console.Write("IMPORT - could not read azimuth information\r\n");
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    rc = Constant.FAILURE;
                }
                else
                {
                    rc = (short)DynFeAzim.FeFetchAzim(cID1, out tempAzim, out nArrayFW);
                }

                if (rc != Constant.SUCCESS)
                {
                    tempAzim = new FeAzim();
                    nArrayFW = NullHelper.CreateArrayOfNullInd(FeAzim.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                    FeValCopy.FeCopyAzim(ref tempAzim, meAzim, ref nArrayFW, nArrayMDB);

                    tempAzim.recstat = "C";
                    tempAzim.deleteall = "N";
                    tempAzim.cmd = cmd;
                    nArrayFW[FeAzim.RECSTAT] = Constant.DB_NOT_NULL;
                    nArrayFW[FeAzim.CMD] = Constant.DB_NOT_NULL;
                    nArrayFW[FeAzim.DELETEALL] = Constant.DB_NOT_NULL;

                    if (DynFeAzim.FeInsertAzim(cID1, tempAzim, nArrayFW) != Constant.SUCCESS)
                    {
                        Console.Write("IMPORT - could not insert azimuth \r\n");
                        ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    }
                }
                DynFeAzim.FeCloseAzim(cID1);

                nRet = DynMeAzim.MeFetchAzim(nHandle, out meAzim, out nArrayMDB);
            }

            if (nRet != Constant.NOMORERECS)
            {
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            DynMeAzim.MeCloseAzim(nHandle);
        }

        /// <summary>
        /// This methods checks whether prescribed site records (selected by the 'whereClause') exist in the PDF's _site table; 
        /// if an individual site record is not in the PDF's _site table then it is copied in from the MDB's me_site table.  
        /// </summary>
        /// <remarks>
        /// The import module will test to see if the requested site records already 
        /// exists in the PDF, if it is not succesful in finding a record it will 
        /// then attempt to copy it from the MDB. If that is unsuccessful then it will 
        /// produce an error message.  
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="whereClause"> - 'where' clause to use in an SQL SELECT query that selects the records.</param>
        /// <param name="cmd"> - MDB operation command code to put in the records.</param>
        public static void FeImportSite(string pdfName,     // pdf display name 
                                        string whereClause,   // 'where' clause to use in an SQL SELECT query 
                                        string cmd)         // command code to put in the record 
        {
            MeSite meSite;
            SQLLEN[] nArrayMDB; //[ME_SITE_SIZE_];
            SQLLEN[] nArrayFW; //[FtSite.SIZE_],
            int rc;
            int sID1;
            string tableName;
            string dynClause;
            FeSite tempSite;

            int nHandle;
            int nRet;

            GenUtil.UtCvtName(Constant.FE_SITE, pdfName, out tableName);

            //...Log2.v(String.Format("\n\nValImport.FeImportSite(): table = {0}; condition = {1}", tableName, whereClause));

            // open site cursor and fetch first record 
            nHandle = DynMeSite.MeSelectSite(whereClause, null);
            if (nHandle < 0)
            {
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                return;
            }

            //	exec sql fetch cmsite into :meSite:nArrayMDB;
            nRet = DynMeSite.MeFetchSite(nHandle, out meSite, out nArrayMDB);

            while (nRet == 0)
            {
                // verify that it is not in the pdf 
                dynClause = String.Format("location = '{0}'", meSite.location);

                if ((sID1 = DynFeSite.FeSelectSite(tableName, dynClause, "")) < 0)
                {
                    Console.Write("IMPORT - could not read site information\r\n");
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    rc = Constant.FAILURE;
                }
                else
                {
                    rc = DynFeSite.FeFetchSite(sID1, out tempSite, out nArrayFW);
                }

                if (rc != Constant.SUCCESS)
                {
                    tempSite = new FeSite();
                    nArrayFW = NullHelper.CreateArrayOfNullInd(FeSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                    FeValCopy.FeCopySite(ref tempSite, meSite, ref nArrayFW, nArrayMDB);

                    tempSite.cmd = cmd;
                    tempSite.recstat = "C";
                    nArrayFW[FeSite.RECSTAT] = Constant.DB_NOT_NULL;
                    nArrayFW[FeSite.CMD] = Constant.DB_NOT_NULL;

                    if (DynFeSite.FeInsertSite(sID1, tempSite, nArrayFW) != Constant.SUCCESS)
                    {
                        Console.Write("IMPORT - could not insert site\r\n");
                        ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    }
                }
                DynFeSite.FeCloseSite(sID1);

                nRet = DynMeSite.MeFetchSite(nHandle, out meSite, out nArrayMDB);
                //exec sql fetch cmsite into :meSite:nArrayMDB;
            }
            if (nRet != Constant.NOMORERECS)
            {
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
            }

            //exec sql close cmsite;
            DynMeSite.MeCloseSite(nHandle);
        }




    }
}
