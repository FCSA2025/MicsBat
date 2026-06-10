# Documented File: Program.cs
**Repository Path:** `SQLtoFlat\Program.cs`
**Primary Layer:** `SQLtoFlat`
**Namespace:** `SQLtoFlat`

## Source Code Representation
```csharp
﻿using System;
using System.IO;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using RuntimeWrapper = Microsoft.SqlServer.Dts.Runtime.Wrapper;

namespace SQLtoFlat
{
    class Program
    {
        static int Main(string[] args)
        {
            string userid = Environment.GetEnvironmentVariable("MicsUser");
            string pwd = Environment.GetEnvironmentVariable("Password");
            string webdrive = Environment.GetEnvironmentVariable("webdrive");
            string datasource = Environment.GetEnvironmentVariable("SqlInstance");

            string dbase = "";         // database
            string user_schema = "";   // user's default schema
            string out_dir = "";       // user's directory
            string sqltable = "";      // sql table source input
            string flatfile = "";      // final flat file output
            string tmpflatfile = "";   // initial flatfile output before substituting # for = 
            string projectCode = "";   // project code
            string prependfile = "";   // full path/name of file to be prepended

            string logfile = webdrive + "\\extractlogs\\" + userid + "SQLtoFlat.txt";

            StreamWriter sw = new StreamWriter(logfile, false);
            sw.WriteLine(DateTime.Now);
            sw.Flush();

            switch (args.Length)
            {
                case 0:
                    Console.WriteLine("Usage: <database> <user's default schema> <user's working directory> <table/view name> <output file name> <project code> <optional prepend file name>");
                    Console.WriteLine("Example: fcsa venn D:\\Inetpub\\micsdev\\mics\\userdirs\\venn\\venn1\\ FCCRunid fccrunidout.csv PCODE1");
                    sw.WriteLine("Usage: <database> <user's default schema> <user's working directory> <table/view name> <output file name> <project code> <optional prepend file name>");
                    sw.WriteLine("Example: fcsa venn D:\\Inetpub\\micsdev\\mics\\userdirs\\venn\\venn1\\ FCCRunid fccrunidout.csv PCODE1");
                    sw.Close();
                    Environment.Exit(0);
                    break;

                case 6:
                    // get argument values
                    dbase = args[0];         // database
                    user_schema = args[1];   // uesr's default schema
                    out_dir = args[2];       // user's directory
                    sqltable = args[3];      // sql table source input
                    flatfile = args[4];      // flat file output
                    projectCode = args[5];   // project code
                    prependfile = "";        // prepend file full name
                    break;

                case 7:
                    dbase = args[0];         // database
                    user_schema = args[1];   // uesr's default schema
                    out_dir = args[2];       // user's directory
                    sqltable = args[3];      // sql table source input
                    flatfile = args[4];      // flat file output
                    projectCode = args[5];   // project code
                    prependfile = args[6];   // prepend file full name
                    break;

                default:
                    Console.WriteLine("Invalid number of arguments");
                    Console.WriteLine("<database> <user's default schema> <user's working directory> <table/view name> <output file name> <project code> <optional prepend file name>");
                    Console.WriteLine("Example: fcsa venn D:\\Inetpub\\micsdev\\mics\\userdirs\\venn\\venn1\\ FCCRunid fccrunidout.csv PCODE1");
                    sw.WriteLine("Invalid number of arguments"); sw.WriteLine("Usage: <database> <user's default schema> <user's working directory> <table/view name> <output file name> <project code> <optional prepend file name>");
                    sw.WriteLine("Example: fcsa venn D:\\Inetpub\\micsdev\\mics\\userdirs\\venn\\venn1\\ FCCRunid fccrunidout.csv PCODE1");
                    sw.Close();
                    Environment.Exit(1);
                    break;
            }

            tmpflatfile = "tmp" + flatfile;

            // check if prepend file exists
            if (prependfile != "")
            {
                if (!File.Exists(prependfile))
                {
                    Console.WriteLine("Prepending file: " + prependfile + " does not exist");
                    sw.WriteLine("Prepending file: " + prependfile + " does not exist");
                    sw.Close();
                    Environment.Exit(2);
                }
            }
            sw.WriteLine("USER        :" + userid);
            sw.WriteLine("DBASE       :" + dbase);
            sw.WriteLine("SCHEMA      :" + user_schema);
            sw.WriteLine("OUT_DIR     :" + out_dir);
            sw.WriteLine("SQLTABLE    :" + sqltable);
            sw.WriteLine("FLATFILE    :" + flatfile);
            sw.WriteLine("PROJECT     :" + projectCode);
            sw.WriteLine("PREPEND FILE:" + prependfile);

            //sw.WriteLine("PWD     :" + pwd);

            sw.WriteLine("Instance:" + datasource);
            sw.Flush();

            Microsoft.SqlServer.Dts.Runtime.Package package = new Microsoft.SqlServer.Dts.Runtime.Package();
            package.Name = userid + "SqlToFlatFile";

            // Add the SQL OLE-DB connection
            ConnectionManager connectionManagerOleDb = package.Connections.Add("OLEDB");
            connectionManagerOleDb.Name = "OLEDB";
            connectionManagerOleDb.ConnectionString =
                "Provider=SQLOLEDB;Data Source=" + datasource + ";Initial Catalog=" + dbase + ";Trusted_Connection=yes;";

            sw.WriteLine("OLEDB connection defined as:");
            sw.WriteLine(connectionManagerOleDb.ConnectionString);
            sw.Flush();

            // Add the Flat File DB connection, basic info only, will define add columns later
            ConnectionManager connectionManagerFlatFile = package.Connections.Add("FLATFILE");
            //prepend tmp to filename as it will be a temporary file produced before = to # conversion
            connectionManagerFlatFile.ConnectionString = out_dir + tmpflatfile;
            connectionManagerFlatFile.Name = "FlatFile";
            connectionManagerFlatFile.Properties["Format"].SetValue(connectionManagerFlatFile, "Delimited");
            connectionManagerFlatFile.Properties["ColumnNamesInFirstDataRow"].SetValue(connectionManagerFlatFile, true);

            sw.WriteLine("FLATFILE connection defined");
            sw.Flush();

            // Add the Data Flow Task 
            package.Executables.Add("STOCK:PipelineTask");

            // Get the task host wrapper, and the Data Flow task
            Microsoft.SqlServer.Dts.Runtime.TaskHost taskHost = package.Executables[0] as Microsoft.SqlServer.Dts.Runtime.TaskHost;
            MainPipe dataFlowTask = (MainPipe)taskHost.InnerObject;

            sw.WriteLine("Dataflow task defined");
            sw.Flush();

            //try
            //{// Add OLE-DB source component
            IDTSComponentMetaData100 componentSource = dataFlowTask.ComponentMetaDataCollection.New();
            componentSource.Name = "OLEDBSource";
            componentSource.ComponentClassID = "Microsoft.OLEDBSource";
            //}
            //catch (Exception e1)
            //{
            sw.WriteLine("componentSource defined");
            sw.Flush();
            //}

            // Get OLE-DB source design-time instance, and initialise component
            CManagedComponentWrapper instanceSource = componentSource.Instantiate();
            instanceSource.ProvideComponentProperties();

            sw.WriteLine("OLEDB connection initialised");
            sw.Flush();

            // Set source connection
            componentSource.RuntimeConnectionCollection[0].ConnectionManagerID = connectionManagerOleDb.ID;
            componentSource.RuntimeConnectionCollection[0].ConnectionManager =
                DtsConvert.GetExtendedInterface(connectionManagerOleDb);

            // Set the source properties
            instanceSource.SetComponentProperty("AccessMode", 2);
            instanceSource.SetComponentProperty("SqlCommand", "SELECT * FROM " + user_schema + "." + sqltable);

            sw.WriteLine("SELECT * FROM " + user_schema + "." + sqltable);
            sw.Flush();

            // Reinitialize the metadata, refresh columns
            instanceSource.AcquireConnections(null);
            instanceSource.ReinitializeMetaData();
            instanceSource.ReleaseConnections();

            sw.WriteLine("OLEDB connection re-initialized");
            sw.Flush();

            // Add Flat File destination
            IDTSComponentMetaData100 componentDestination = dataFlowTask.ComponentMetaDataCollection.New();
            componentDestination.Name = "FlatFileDestination";
            componentDestination.ComponentClassID = "Microsoft.FlatFileDestination";

            // Get Flat File destination design-time instance, and initialise component
            CManagedComponentWrapper instanceDestination = componentDestination.Instantiate();
            instanceDestination.ProvideComponentProperties();

            // Set destination connection
            componentDestination.RuntimeConnectionCollection[0].ConnectionManagerID = connectionManagerFlatFile.ID;
            componentDestination.RuntimeConnectionCollection[0].ConnectionManager =
                DtsConvert.GetExtendedInterface(connectionManagerFlatFile);

            IDTSPath100 path = dataFlowTask.PathCollection.New();
            path.AttachPathAndPropagateNotifications(componentSource.OutputCollection[0],
                componentDestination.InputCollection[0]);


            // Get input and virtual input for destination to select and map columns
            IDTSInput100 destinationInput = componentDestination.InputCollection[0];
            IDTSVirtualInput100 destinationVirtualInput = destinationInput.GetVirtualInput();
            IDTSVirtualInputColumnCollection100 destinationVirtualInputColumns =
                destinationVirtualInput.VirtualInputColumnCollection;

            // Get native flat file connection 
            RuntimeWrapper.IDTSConnectionManagerFlatFile100 connectionFlatFile =
                connectionManagerFlatFile.InnerObject as RuntimeWrapper.IDTSConnectionManagerFlatFile100;

            // Create flat file connection columns to match pipeline
            int indexMax = destinationVirtualInputColumns.Count - 1;
            for (int index = 0; index <= indexMax; index++)
            {
                // Get input column to replicate in flat file
                IDTSVirtualInputColumn100 virtualInputColumn = destinationVirtualInputColumns[index];

                // Add column to Flat File connection manager
                RuntimeWrapper.IDTSConnectionManagerFlatFileColumn100 flatFileColumn =
                    connectionFlatFile.Columns.Add() as RuntimeWrapper.IDTSConnectionManagerFlatFileColumn100;
                flatFileColumn.ColumnType = "Delimited";
                flatFileColumn.ColumnWidth = virtualInputColumn.Length;
                flatFileColumn.DataPrecision = virtualInputColumn.Precision;
                flatFileColumn.DataScale = virtualInputColumn.Scale;
                flatFileColumn.DataType = virtualInputColumn.DataType;
                RuntimeWrapper.IDTSName100 columnName = flatFileColumn as RuntimeWrapper.IDTSName100;
                columnName.Name = virtualInputColumn.Name;

                if (index < indexMax)
                    flatFileColumn.ColumnDelimiter = ",";
                else
                    flatFileColumn.ColumnDelimiter = Environment.NewLine;
            }

            // Reinitialize the metadata, generating external columns from flat file columns
            instanceDestination.AcquireConnections(null);
            instanceDestination.ReinitializeMetaData();
            instanceDestination.ReleaseConnections();

            sw.WriteLine("Columns generated");
            sw.Flush();

            // Select and map destination columns
            foreach (IDTSVirtualInputColumn100 virtualInputColumn in destinationVirtualInputColumns)
            {
                // Select column, and retain new input column
                IDTSInputColumn100 inputColumn = instanceDestination.SetUsageType(destinationInput.ID, destinationVirtualInput, virtualInputColumn.LineageID, DTSUsageType.UT_READONLY);
                // Find external column by name
                IDTSExternalMetadataColumn100 externalColumn = destinationInput.ExternalMetadataColumnCollection[inputColumn.Name];
                // Map input column to external column
                instanceDestination.MapInputColumn(destinationInput.ID, inputColumn.ID, externalColumn.ID);
            }

            sw.WriteLine("Mappings complete");
            sw.Flush();

#if DEBUG
            // Save package to disk, DEBUG only
            //new Microsoft.SqlServer.Dts.Runtime.Application().SaveToXml(String.Format(@"C:\Temp\{0}.dtsx", package.Name), package, null);
            //new Microsoft.SqlServer.Dts.Runtime.Application().SaveToXml(String.Format(out_dir + package.Name + ".dtsx", package.Name), package, null);
            //Console.WriteLine(@"C:\Temp\{0}.dtsx", package.Name);
#endif
            sw.WriteLine("Starting execute");
            sw.Flush();

            package.Execute();

            sw.WriteLine("Execute ended");
            sw.Flush();

            int errcount = 0;
            foreach (DtsError error in package.Errors)
            {
                //Console.WriteLine("ErrorCode       : {0}", error.ErrorCode);
                //Console.WriteLine("  SubComponent  : {0}", error.SubComponent);
                //Console.WriteLine("  Description   : {0}", error.Description);
                errcount++;
                sw.WriteLine("ErrorCode     :" + error.ErrorCode);
                sw.WriteLine("SubComponent  :" + error.SubComponent);
                sw.WriteLine("Description   :" + error.Description);
                sw.Flush();
            }

            package.Dispose();

            string tmpflatout = "";
            // replace '=' with '#' in output file
            if (File.Exists(out_dir + tmpflatfile))
            {
                Console.WriteLine("File: " + out_dir + tmpflatfile + " created");
                // read entire file into a string
                string strtmpflatfile = File.ReadAllText(out_dir + tmpflatfile);
                Console.WriteLine("All text read from " + out_dir + tmpflatfile + " into string");
                // replace '=' in string with '#'
                tmpflatout = strtmpflatfile.Replace('=', '#');
                Console.WriteLine("Characters replaced in string");

            }
            else
            {
                Console.WriteLine("File: " + out_dir + tmpflatfile + " not created");
                Environment.Exit(3);
                sw.Close();
            }

            string strprepend = "";
            if (prependfile != "")
            {
                // read prepend file text into string
                strprepend = File.ReadAllText(prependfile);
            }

            // write resulting strings to final output file
            File.WriteAllText(out_dir + flatfile, strprepend + "\n" + tmpflatout);
            Console.WriteLine("String written to: " + out_dir + tmpflatfile);
            Console.WriteLine("File: " + out_dir + flatfile + " written");

            sw.Close();
            return errcount;
        }
    }
}


```
