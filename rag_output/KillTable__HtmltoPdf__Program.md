# Documented File: Program.cs
**Repository Path:** `KillTable\HtmltoPdf\Program.cs`
**Primary Layer:** `KillTable`
**Namespace:** `HtmltoPdf`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BuildRadioCatalog;

namespace HtmltoPdf
{
    class Program
    {
        static void Main(string[] args)
        {
            //string userid = Environment.GetEnvironmentVariable("MicsUser");
            //string webdrive = Environment.GetEnvironmentVariable("webdrive");

            string logfile = "C:\\extractlogs\\venn1HtmltoPdf.txt";

            StreamWriter sw = new StreamWriter(logfile, false);
            sw.WriteLine(DateTime.Now);
            sw.Flush();

            string htmfile = "C:\\\\Inetpub\\\\micstest\\\\mics\\\\userdirs\\\\venn\\\\venn1\\\\RadioCat\\\\Subsidiary\\\\SUBante4.htm";
            int header_height = 112;

            // get argument values
            //string htmfile = args[0];         // html file to be converted
            //string header_height = args[1];   // height of header in points
            //string header_subtext = args[2];  // subtext for header
            //string reptype = args[3];         // report type
            //string repno = args[4];           // report number

            sw.WriteLine("htmfile    :" + htmfile);
            sw.WriteLine("header_height   :" + header_height);
            //sw.WriteLine("header_subtext:" + header_subtext);
            //sw.WriteLine("reptype:" + reptype);
            //sw.WriteLine("repno :" + repno);
            //sw.Flush();
            sw.Close();

 
            string converror = common.ConvertToPdfSUB(htmfile, header_height, "CODED ANTENNA PATTERN TABLES", "ANTE", 4);

        }
    }
}

```
