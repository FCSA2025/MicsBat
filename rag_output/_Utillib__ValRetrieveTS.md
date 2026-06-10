# Documented File: ValRetrieveTS.cs
**Repository Path:** `_Utillib\ValRetrieveTS.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using _NewLib;
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;  //Invented to mimic (char *)
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
    /// Provides methods to retrieve the first site, antenna or channel record from the 
    /// database tables <b>main.mt_ste, </b><b>main.mt_ante</b> or <b>main.mt_chan</b> that
    /// matches prescribed SQL 'WHERE' search criteria and 'ORDER BY' clauses.
    /// The corresponding column nullInd information is also retrieved.
    /// </summary>
    public class ValRetrieveTS
    {
#if PINVOKE
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int ftValRetrieveMDBSite([In, Out] MtSite mtSite, string whereClause, [In, Out] SQLLEN[] nArrayMDB);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int ftValRetrieveMDBAntenna([In, Out] MtAnte mtAnte, string whereClause, [In, Out] SQLLEN[] nArrayMDB);
     
        public static int FtValRetrieveMDBSite_NATIVE(out MtSite mtSite, string whereClause, out SQLLEN[] nArrayMDB)
        {
            // Satisfy 'out' requirements.
            mtSite = new MtSite();
            nArrayMDB = NullHelper.CreateArrayOfNullInd(MtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            int nRet = 666;

            nRet = ftValRetrieveMDBSite(mtSite, whereClause, nArrayMDB);

            return nRet;
        }
        
        public static int FtValRetrieveMDBAntenna_NATIVE(out MtAnte mtAnte, string whereClause, out SQLLEN[] nArrayMDB)
        {
            // Satisfy 'out' requirements.
            mtAnte = new MtAnte();
            nArrayMDB = NullHelper.CreateArrayOfNullInd(MtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            int nRet = 666;

            nRet = ftValRetrieveMDBAntenna(mtAnte, whereClause, nArrayMDB);

            return nRet;
        }                   
#endif
        //----------------------------------------------------------------

        /// <summary>
        /// Retrieves the first site record from the database table <b>main.mt_site</b> that
        /// matches the prescribed SQL 'WHERE' search clause. The 
        /// corresponding column nullInd information is also retrieved.
        /// </summary>
        /// <param name="mtSite"> - populated MtSite object.</param>
        /// <param name="whereClause"> - parameters to follow the SQL 'WHERE' token.</param>
        /// <param name="nArrayMDB"> - populated array of ODBC nullInds.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        public static int FtValRetrieveMDBSite(out MtSite mtSite, string whereClause, out SQLLEN[] nArrayMDB)
        {
            //...Log2.v("\nValRetrieveTS.FtValRetrieveMDBSite(): DynMdbSite.TableName = " + DynMdbSite.TableName);

            int nHandle = DynMdbSite.MtSelectSite(whereClause, null);
            int nRet = DynMdbSite.MtFetchSite(nHandle, out mtSite, out nArrayMDB);
            DynMdbSite.MtCloseSite(nHandle);

            return (nRet);
        }

        /// <summary>
        /// Retrieves the first antenna record from the database table <b>main.mt_ante</b> that
        /// matches the prescribed SQL 'WHERE' search clause.
        ///  The corresponding column nullInd information is also retrieved.
        /// </summary>
        /// <param name="mtAnte"> - populated MtAnte object.</param>
        /// <param name="whereClause"> - parameters to follow the SQL 'WHERE' token.</param>
        /// <param name="nArrayMDB"> - populated array of ODBC nullInds.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        public static int FtValRetrieveMDBAntenna(out MtAnte mtAnte, string whereClause, out SQLLEN[] nArrayMDB)
        {
            int nHandle = DynMdbAntenna.MtSelectAntenna(whereClause, null);
            int nRet = DynMdbAntenna.MtFetchAntenna(nHandle, out mtAnte, out nArrayMDB);
            DynMdbAntenna.MtCloseAntenna(nHandle);
            return (nRet);
        }

        /// <summary>
        /// Retrieves the first channel record from the database table <b>main.mt_chan</b> that
        /// matches the prescribed SQL 'WHERE' search criteria and 'ORDER BY' clauses.
        /// The corresponding column nullInd information is also retrieved.
        /// </summary>
        /// <param name="mtChan"> - populated MtChan object.</param>
        /// <param name="whereClause"> - parameters to follow the SQL 'WHERE' token.</param>
        /// <param name="nArrayMDB"> - populated array of ODBC nullInds.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        public static int FtValRetrieveMDBChannel(out MtChan mtChan, string whereClause, out SQLLEN[] nArrayMDB)
        {
            int nHandle = DynMdbChannel.MtSelectChannel(whereClause, "");
            int nRet = DynMdbChannel.MtFetchChannel(nHandle, out mtChan, out nArrayMDB);

            DynMdbChannel.MtCloseChannel(nHandle);

            return (nRet);
        }




    }
}

```
