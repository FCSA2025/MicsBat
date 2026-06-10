# Documented File: DbRecAsString.cs
**Repository Path:** `_Utillib\DbRecAsString.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
﻿using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using _Configuration;
    using _NewLib;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides convenience methods that fetch records from the Main, 
    /// Subsidiary and/or User tables and returns it as a formatted string comprising all 
    /// requested DB record column values and associated nullInds.
    /// </summary>
    public class DbRecAsString
    {
        /// <summary>
        /// This method returns a formatted string comprising all column and nullInd values
        /// for a single record from the User table ft_XXX_site where XXX is the PDF name;
        /// the (FtSite) record is uniquely identified by its key value (call1).
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="pdfName"></param>
        /// <param name="call1"></param>
        /// <returns></returns>
        public static string FtSiteByKey(string prefix, string pdfName, string call1)
        {
            FtSite ftSite;
            SQLLEN[] nullInds;
            bool found;
            int rc;
            string message = "";

            string header = String.Format("\n\n{0}: FtTableSpy.SiteRecordByKey(): ftSite for call1 = {1} : ", prefix, call1);

            rc = DynSite.FetchFtSiteByKey(pdfName, call1, out ftSite, out nullInds, out found);

            if (rc != Constant.SUCCESS)
            {
                message = header + "call to DynSite.FetchFtSiteByKey() failed.";
            }
            else
            {
                if (!found)
                {
                    message = header + "Record not Found.";
                }
                else
                {
                    message = header + ftSite.ToStringWN(nullInds);
                }
            }

            return message;
        }

        /// <summary>
        /// This method returns a formatted string comprising all column and nullInd values
        /// for a single record from the User table ft_XXX_ante where XXX is the PDF name;
        /// the (FtAnte) record is uniquely identified by its key values (call1, call2, bndcde, anum).
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="pdfName"></param>
        /// <param name="call1"></param>
        /// <param name="call2"></param>
        /// <param name="bndcde"></param>
        /// <param name="anum"></param>
        /// <returns></returns>
        public static string FtAnteByKey(string prefix, string pdfName, string call1, string call2, string bndcde, short anum)
        {
            FtAnte ftAnte;
            SQLLEN[] nullInds;
            bool found;
            int rc;
            string message = "";

            string header = String.Format("\n\n{0}: FtTableSpy.AnteRecordByKey(): ftAnte for key = {1} : {2} : {3} : {4}",
                                            prefix, call1, call2, bndcde, anum);

            rc = DynAntenna.FetchFtAnteByKey(pdfName, call1, call2, bndcde, anum, out ftAnte, out nullInds, out found);

            if (rc != Constant.SUCCESS)
            {
                message = header + "call to DynAntenna.FetchFtAnteByKey() failed.";
            }
            else
            {
                if (!found)
                {
                    message = header + "Record not Found.";
                }
                else
                {
                    message = header + ftAnte.ToStringWN(nullInds);
                }
            }

            return message;
        }

        /// <summary>
        /// This method returns a formatted string comprising all column and nullInd values
        /// for a single record from the User table ft_XXX_chan where XXX is the PDF name;
        /// the (FtChan) record is uniquely identified by its key value (call1, call2, bndcde, chid).
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="pdfName"></param>
        /// <param name="call1"></param>
        /// <param name="call2"></param>
        /// <param name="bndcde"></param>
        /// <param name="chid"></param>
        /// <returns></returns>
        public static string FtChanByKey(string prefix, string pdfName, string call1, string call2, string bndcde, string chid)
        {
            FtChan ftChan;
            SQLLEN[] nullInds;
            bool found;
            int rc;
            string message = "";

            string header = String.Format("\n\n{0}: FtTableSpy.ChanRecordByKey(): ftChan for key = {1} : {2} : {3} : {4}",
                                            prefix, call1, call2, bndcde, chid);

            rc = DynChannel.FetchFtChanByKey(pdfName, call1, call2, bndcde, chid, out ftChan, out nullInds, out found);

            if (rc != Constant.SUCCESS)
            {
                message = header + "call to DynChannel.FetchFtChanByKey() failed.";
            }
            else
            {
                if (!found)
                {
                    message = header + "Record not Found.";
                }
                else
                {
                    message = header + ftChan.ToStringWN(nullInds);
                }
            }

            return message;
        }


    }
}

```
