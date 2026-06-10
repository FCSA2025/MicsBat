# Documented File: ProdBinConfig.cs
**Repository Path:** `_DataStructures\ProdBinConfig.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace _DataStructures
{
    using SQLHANDLE = IntPtr;
    using SQLLEN = Int64;
    using SQLPOINTER = IntPtr;
    using _Configuration;
    using System.Runtime.InteropServices;
    using _NewLib;

    /// <summary>
    /// This class encapsulates the methods required to compare the current contents of
    /// d:\\prod\\bin on the local Windows host to the d:\\prod\\bin contents on another network
    /// server.
    /// </summary>
    public class ProdBinConfig
    {
        private string fileName;
        private string custodian;
        private string lastModified;
        private string sortDate;
        private string md5;
        private string description;

        //The total number of fields corresponding to database columns.
        public const int NUM_COLUMNS = 6;

        public const int FILENAME = 0;
        public const int CUSTODIAN = 1;
        public const int LASTMODIFIED = 2;
        public const int SORTDATE = 3;
        public const int MD5 = 4;
        public const int DESCRIPTION = 5;

        public const int FILENAME_SZ = 40 + 1;
        public const int CUSTODIAN_SZ = 20 + 1;
        public const int LASTMODIFIED_SZ = 19 + 1;
        public const int SORTDATE_SZ = 8 + 1;
        public const int MD5_SZ = 32 + 1;
        public const int DESCRIPTION_SZ = 1000 + 1;

        //Array of strings providing the class-member / database-column names.
        private static string[] columnNames = new string[NUM_COLUMNS] { "fileName", "custodian", "lastModified", "sortDate", "md5", "description" };

        public string FileName { get { return fileName; } set { fileName = value; } }
        public string Custodian { get { return custodian; } set { custodian = value; } }
        public string LastModified { get { return lastModified; } set { lastModified = value; } }
        public string MD5hash { get { return md5; } set { md5 = value; } }
        public string Description { get { return description; } set { description = value; } }
        public string SortDate { get { return sortDate; } set { sortDate = value; } }

        private ProdBinConfig() { }

        public ProdBinConfig(string fileName, string md5)
        {
            this.fileName = fileName;
            this.md5 = md5;
            custodian = "";
            lastModified = "";
            description = "";
            sortDate = "";
        }

        /// <summary>
        /// This method returns an CSV string that provides the current member values of this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0},{1},{2},{3},{4},{5}", fileName, custodian, lastModified, sortDate, md5, description);
        }

        /// <summary>
        /// This method returns an CSV string that provides the current member values of this object; 
        /// the individual CSV fields are wrapped in single quote marks useful for inclusion in an SQL query..
        /// </summary>
        /// <returns></returns>
        public string ToStringAsQuotedCSV()
        {
            return String.Format("'{0}','{1}','{2}','{3}','{4}','{5}'", fileName, custodian, lastModified, sortDate, md5, description);
        }

        /// <summary>
        /// This method returns a CSV string providing the current member values of this object.
        /// </summary>
        /// <returns></returns>
        public static string CSVheader()
        {
            return "FileName, Custodian, LastModified, Sortdate, MD5, Description";
        }

        /// <summary>
        /// This method is passed a List&lt;ProdBinConfig&gt; and returns the first object
        /// that has its fileName member equal to a prescribed string.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="pbcList"></param>
        /// <returns></returns>
        public static ProdBinConfig GetProdBinConfigByName(string fileName, List<ProdBinConfig> pbcList)
        {
            ProdBinConfig pbc = null;

            foreach (ProdBinConfig fMD in pbcList)
            {
                if (fMD.fileName.ToLower().Equals(fileName.ToLower()))
                {
                    pbc = fMD;
                    break;
                }
            }

            return pbc;
        }


        /// <summary>
        /// This method returns an array of IntPtr that point to the start addresses of possibly 
        /// non-contiguous blocks of global (heap) memory, each of a sufficient size to hold the 
        /// value of a specific member of this object; each of the pointers is then registered
        /// with ODBC as a 'binding'. Similarly for the nullInd array associated with this object.
        /// Thus, this method creates the parameter bindings prior to an SQL / ODBC 'fetch', 'update' or 
        /// 'insert' query.        /// </summary>
        /// <param name="hStmt"></param>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        public static void BindPtrsToCols(SQLHANDLE hStmt, out SQLPOINTER[] tgtValPtrs, out SQLPOINTER[] nullIndPtrs)
        {
            tgtValPtrs = new SQLPOINTER[NUM_COLUMNS];
            nullIndPtrs = new SQLPOINTER[NUM_COLUMNS];

            tgtValPtrs[FILENAME] = Marshal.AllocHGlobal(FILENAME_SZ + 1);
            nullIndPtrs[FILENAME] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, FILENAME + 1, tgtValPtrs[FILENAME], ProdBinConfig.FILENAME_SZ, nullIndPtrs[FILENAME]);

            tgtValPtrs[CUSTODIAN] = Marshal.AllocHGlobal(CUSTODIAN_SZ + 1);
            nullIndPtrs[CUSTODIAN] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, CUSTODIAN + 1, tgtValPtrs[CUSTODIAN], ProdBinConfig.CUSTODIAN_SZ, nullIndPtrs[CUSTODIAN]);

            tgtValPtrs[LASTMODIFIED] = Marshal.AllocHGlobal(LASTMODIFIED_SZ + 1);
            nullIndPtrs[LASTMODIFIED] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, LASTMODIFIED + 1, tgtValPtrs[LASTMODIFIED], ProdBinConfig.LASTMODIFIED_SZ, nullIndPtrs[LASTMODIFIED]);

            tgtValPtrs[SORTDATE] = Marshal.AllocHGlobal(SORTDATE_SZ + 1);
            nullIndPtrs[SORTDATE] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, SORTDATE + 1, tgtValPtrs[SORTDATE], ProdBinConfig.SORTDATE_SZ, nullIndPtrs[SORTDATE]);

            tgtValPtrs[MD5] = Marshal.AllocHGlobal(MD5_SZ + 1);
            nullIndPtrs[MD5] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, MD5 + 1, tgtValPtrs[MD5], ProdBinConfig.MD5_SZ, nullIndPtrs[MD5]);

            tgtValPtrs[DESCRIPTION] = Marshal.AllocHGlobal(DESCRIPTION_SZ + 1);
            nullIndPtrs[DESCRIPTION] = Marshal.AllocHGlobal(sizeof(SQLLEN));
            Bind.BindBufferToString(hStmt, DESCRIPTION + 1, tgtValPtrs[DESCRIPTION], ProdBinConfig.DESCRIPTION_SZ, nullIndPtrs[DESCRIPTION]);
        }




        /// <summary>
        /// This method can be used after a call to ODBC.SQLFetch() in which the
        /// result-set is written to parameter-binding buffers in global (heap)
        /// memory; the method returns a deep-cloned ProdBinConfig object and its associated nullInds array.
        /// </summary>
        /// <param name="tgtValPtrs"></param>
        /// <param name="nullIndPtrs"></param>
        /// <param name="prodBinConfig"></param>
        /// <param name="nullInds"></param>
        public static void ReadColBindings(SQLPOINTER[] tgtValPtrs, SQLPOINTER[] nullIndPtrs, out ProdBinConfig prodBinConfig, out SQLLEN[] nullInds)
        {
            prodBinConfig = new ProdBinConfig();
            nullInds = NullHelper.CreateArrayOfNullInd(ProdBinConfig.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            float[] F = new float[1];
            double[] D = new double[1];

            SQLLEN nullInd;

            nullInd = Marshal.ReadInt64(nullIndPtrs[FILENAME]);
            if (nullInd == Constant.DB_NULL)
            {
                prodBinConfig.fileName = "";
                nullInds[FILENAME] = Constant.DB_NULL;
            }
            else
            {
                prodBinConfig.fileName = Marshal.PtrToStringAnsi(tgtValPtrs[FILENAME]).Trim();
                nullInds[FILENAME] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[CUSTODIAN]);
            if (nullInd == Constant.DB_NULL)
            {
                prodBinConfig.custodian = "";
                nullInds[CUSTODIAN] = Constant.DB_NULL;
            }
            else
            {
                prodBinConfig.custodian = Marshal.PtrToStringAnsi(tgtValPtrs[CUSTODIAN]).Trim();
                nullInds[CUSTODIAN] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[LASTMODIFIED]);
            if (nullInd == Constant.DB_NULL)
            {
                prodBinConfig.lastModified = "";
                nullInds[LASTMODIFIED] = Constant.DB_NULL;
            }
            else
            {
                prodBinConfig.lastModified = Marshal.PtrToStringAnsi(tgtValPtrs[LASTMODIFIED]).Trim();
                nullInds[LASTMODIFIED] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[SORTDATE]);
            if (nullInd == Constant.DB_NULL)
            {
                prodBinConfig.sortDate = "";
                nullInds[SORTDATE] = Constant.DB_NULL;
            }
            else
            {
                prodBinConfig.sortDate = Marshal.PtrToStringAnsi(tgtValPtrs[SORTDATE]).Trim();
                nullInds[SORTDATE] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[MD5]);
            if (nullInd == Constant.DB_NULL)
            {
                prodBinConfig.md5 = "";
                nullInds[MD5] = Constant.DB_NULL;
            }
            else
            {
                prodBinConfig.md5 = Marshal.PtrToStringAnsi(tgtValPtrs[MD5]).Trim();
                nullInds[MD5] = Constant.DB_NOT_NULL;
            }

            nullInd = Marshal.ReadInt64(nullIndPtrs[DESCRIPTION]);
            if (nullInd == Constant.DB_NULL)
            {
                prodBinConfig.description = "";
                nullInds[DESCRIPTION] = Constant.DB_NULL;
            }
            else
            {
                prodBinConfig.description = Marshal.PtrToStringAnsi(tgtValPtrs[DESCRIPTION]).Trim();
                nullInds[DESCRIPTION] = Constant.DB_NOT_NULL;
            }

        }






        /// <summary>
        /// This method returns the member values of an object formatted
        /// as a string of columnName = 'value' expressions that can be used
        /// in the SET clause of an SQL UPDATE query.
        /// </summary>
        /// <param name="nullInds"> - array of ODBC null indicators.</param>
        /// <returns></returns>
        public string ToStringAsCSVequates(SQLLEN[] nullInds)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(columnNames[ProdBinConfig.FILENAME] + "=" + (nullInds[ProdBinConfig.FILENAME] == Constant.DB_NULL ? "NULL," : "'" + fileName.ToString() + "',"));
            sb.Append(columnNames[ProdBinConfig.CUSTODIAN] + "=" + (nullInds[ProdBinConfig.CUSTODIAN] == Constant.DB_NULL ? "NULL," : "'" + custodian.ToString() + "',"));
            sb.Append(columnNames[ProdBinConfig.LASTMODIFIED] + "=" + (nullInds[ProdBinConfig.LASTMODIFIED] == Constant.DB_NULL ? "NULL," : "'" + lastModified.ToString() + "',"));
            sb.Append(columnNames[ProdBinConfig.SORTDATE] + "=" + (nullInds[ProdBinConfig.SORTDATE] == Constant.DB_NULL ? "NULL," : "'" + sortDate.ToString() + "',"));
            sb.Append(columnNames[ProdBinConfig.MD5] + "=" + (nullInds[ProdBinConfig.MD5] == Constant.DB_NULL ? "NULL," : "'" + md5.ToString() + "',"));
            sb.Append(columnNames[ProdBinConfig.DESCRIPTION] + "=" + (nullInds[ProdBinConfig.DESCRIPTION] == Constant.DB_NULL ? "NULL" : "'" + description.ToString() + "'"));

            return sb.ToString();
        }




        public const string CREATE_TABLE = "" +
"CREATE TABLE [hulme].[prod_bin_config] (" +
"[fileName] [varchar](40) NOT NULL," +
"[custodian] [varchar](20) NOT NULL," +
"[lastModified] [varchar](19) NOT NULL," +
"[sortDate] [varchar](8) NOT NULL," +
"[md5] [varchar](32) NOT NULL," +
"[description] [varchar](1000) NOT NULL," +
"CONSTRAINT [PK_ProdBinConfig] PRIMARY KEY CLUSTERED " +
"(" +
"[fileName] ASC," +
"[md5] ASC" +
")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]" +
") ON [PRIMARY]";
    }
}

```
