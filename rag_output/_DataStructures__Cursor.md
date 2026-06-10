# Documented File: Cursor.cs
**Repository Path:** `_DataStructures\Cursor.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using _Configuration;
using _NewLib;

namespace _DataStructures
{

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
    /// Encapsulates the functionality of a generic ODBC record-set cursor to support table-specific 
    /// database row select, fetch, update, insert and delete operations.
    /// </summary>
    //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class Cursor
    {
        //Universal Ft cursor members.
        public SQLHDBC hConn;
        public SQLHANDLE hStmt;
        //[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.TABLE_NM_SZ)]
        public string tableName;
        //[MarshalAsAttribute(UnmanagedType.U1)]
        public bool cursorOpen;
        //[MarshalAsAttribute(UnmanagedType.U1)]
        public bool pastLastRow;
        //[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.DB_CALL_SZ)]
        public string cCurrentCall1;
        //[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.DB_CALL_SZ)]
        public string cCurrentCall2;
        //[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = Constant.BNDCDE_SZ)]
        public string cCurrentBndcde;
        //[MarshalAsAttribute(UnmanagedType.I2)]

        //Unique to sites.
        public string cCurrentCall;

        //Unique to antennae.
        public short nCurrentAnum;

        //Unique to channels.
        public string cCurrentChid;

        //Unique to TpDynParm.
        public int nRowNumber;

        //AH: added to facilitate testing.
        public string sqlQuery;

        //AH: added for DynFeSite.FeFetchSite()
        public string cCurrentLoc;

        //AH: added for DynFeAzim.FeFetchAzim()
        public string cLocation;
        public string cCall1;
        public float fAzim;

        //AH: added for SuDynAnte.SuAnteFetch()
        public string cCurrAcode;
        public SQLHANDLE hUpdate;

        //AH: added for SuDynAnte.SuAntdFetch()
        public float fCurrAntang;

        //AH: added for testing that 'select' --> 'fetch' 
        //calls are contiguous (i.e. they are a matched pair).
        public int magicNumber;

        //-------------------------------------------------------------------------------------

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public Cursor()
        {
            hConn = SQLHDBC.Zero;
            hStmt = SQLHANDLE.Zero;
            tableName = null;
            cursorOpen = false;
            pastLastRow = false;
            cCurrentCall = null;
            cCurrentCall1 = null;
            cCurrentCall2 = null;
            cCurrentBndcde = null;
            nCurrentAnum = 0;
            cCurrentChid = null;
            sqlQuery = null;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("hConn =           " + "0x" + hConn.ToString("x16"));
            sb.AppendLine("hStmt =           " + "0x" + hStmt.ToString("x16"));
            sb.AppendLine("tableName =       " + tableName);
            sb.AppendLine("cursorOpen =      " + cursorOpen);
            sb.AppendLine("pastLastRow =     " + pastLastRow);
            sb.AppendLine("cCurrentCall =    " + cCurrentCall1);
            sb.AppendLine("cCurrentCall1 =   " + cCurrentCall1);
            sb.AppendLine("cCurrentCall2 =   " + cCurrentCall2);
            sb.AppendLine("cCurrentBndcde =  " + cCurrentBndcde);
            sb.AppendLine("nCurrentAnum =    " + nCurrentAnum);
            sb.AppendLine("cCurrentChid =    " + cCurrentChid);

            sb.AppendLine("sqlQuery =        " + sqlQuery);
            return sb.ToString();
        }

    }
}

```
