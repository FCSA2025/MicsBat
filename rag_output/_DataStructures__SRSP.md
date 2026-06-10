# Documented File: SRSP.cs
**Repository Path:** `_DataStructures\SRSP.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    public class SRSP
    {
        public string srspID = "";                // key.
        public int span = 0;                    // key.
        public int _of = 0;
        public int freqLoKHz = int.MinValue;
        public int freqHiKHz = int.MinValue;
        public string title = "";
        public bool applicMICS = false;
        public string pubDate = "";
        public string html_url = "";
        public string pdf_url = "";

        //===========================================================================

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 10;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "srspID", "span", "_of", "freqLoKHz", "freqHiKHz", "title", "applicMICS", "pubDate", "html_url", "pdf_url" };

        public const string AllColumnsForSqlSelect = " srspID, span, _of, freqLoKHz, freqHiKHz, title, applicMICS, pubDate, html_url, pdf_url ";

        public const int SRSPID = 0;
        public const int SPAN = 1;
        public const int _OF = 2;
        public const int FREQLOKHZ = 3;
        public const int FREQHIKHZ = 4;
        public const int TITLE = 5;
        public const int APPLICMICS = 6;
        public const int PUBDATE = 7;
        public const int HTML_URL = 8;
        public const int PDF_URL = 9;

        public const int SRSP_SZ = 17;
        public const int TITLE_SZ = 501;
        public const int PUBDATE_SZ = 101;
        public const int HTML_URL_SZ = 501;
        public const int PDF_URL_SZ = 501;

        //===========================================================================

        /// <summary>
        /// This method returns an annotated multi-line string that shows the current values
        /// of the member variables of this SRSP object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n ===== SRSP ===== ");

            sb.Append("\nsrsp = " + srspID);
            sb.Append("\nspan = " + span);
            sb.Append("\n_of = " + _of);
            sb.Append("\nfreqLoKHz = " + freqLoKHz);
            sb.Append("\nfreqHiKHz = " + freqHiKHz);
            sb.Append("\ntitle = " + title);
            sb.Append("\napplicMICS = " + applicMICS);
            sb.Append("\npubDate = " + pubDate);
            sb.Append("\nhtml_url = " + html_url);
            sb.Append("\npdf_url = " + pdf_url);

            return sb.ToString();
        }

        /// <summary>
        /// This method returns a CSV string that shows the current values of a subset
        /// of the member variables of this SRSP object.
        /// </summary>
        /// <returns></returns>
        public string ToStringBrief()
        {
            return String.Format("{0}, {1}, {2}, {3}, {4}", srspID, span, _of, freqLoKHz, freqHiKHz);
        }





    }
}

```
