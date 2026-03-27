using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using SQLRETURN = Int16;
    using SQLHANDLE = IntPtr;
    using SQLLEN = Int64;
    using _Configuration;
    using _NewLib;
    using _DataStructures;
    using System.IO;
    public class AtRecordChng
    {

        /// <summary>
        /// This method adds a record to the DB adm.audit_trail table to catalogue a change
        /// made to the MDB or SDB; a copy of the flat file PDF or SDF is stored in a uniquely 
        /// named file in one of the folders /prod/bin/audit/es/ or /prod/bin/audit/band/ etc.
        /// </summary>
        /// <param name="dbaseName"></param>
        /// <param name="pdfName"></param>
        /// <param name="tableType"></param>
        public static void AtRecordChange(string dbaseName,     /* data base connected to */
                                            string pdfName,    /* display PDF or SDF name */
                                            int tableType)      /* table type ie SU_BAND, FE,  etc. */
        {
            /* Local variables */
            int rc;                 /* return code */
            string curDate;         /* current date */
            string curTime;         /* current time */
            string tabTypeNm;       /* table type ie "band" etc. */
            string tmpPath;         /* path & file name - flatfile*/
            string auditPath;       /* path & file name - flatfile*/
            string printPgmNm;      /* print routine for ForkEm */
            string tmpListFile;

            // AuditTrail objects.
            AuditTrail auditTrail = new AuditTrail();
            SQLLEN[] nullInds = NullHelper.CreateArrayOfNullInd(AuditTrail.NUM_COLUMNS, NullHelper.ColumnStatus.NOT_NULL);

            // Audit parameters.
            //string auditUltrixId;
            //string auditMicsId;
            //string auditTabletype;
            //string auditFilename;
            //string auditDate;
            //string auditTime;

            // Windows Command Shell parameters.
            string clArgs;
            string cmd;
            string stdOutTxt;
            string stdErrTxt;
            int exitCode;

            /* display name for ForkEm */
            string[] dispName = new string[7] { "", "", "", "", "", "", "" };

            UserInfoData userStuff; /* structure of user info */

            SQLHANDLE hConn = Ssutil.NewConn();

            GenUtil.UtGetDateTime(out curDate, out curTime);
            auditTrail.mdate = curDate;
            auditTrail.mtime = curTime;

            tmpPath = Ssutil.GetFcsaTemp();

            switch (tableType)
            {
                case Constant.FE:
                    tabTypeNm = "es";
                    break;
                case Constant.FT:
                    tabTypeNm = "ts";
                    break;
                case Constant.SU_ANTE:
                    tabTypeNm = "ante";
                    break;
                case Constant.SU_BAND:
                    tabTypeNm = "band";
                    break;
                case Constant.SU_CTX:
                    tabTypeNm = "ctx";
                    break;
                case Constant.SU_EQPT:
                    tabTypeNm = "eqpt";
                    break;
                case Constant.SU_NOTE:
                    tabTypeNm = "note";
                    break;
                case Constant.SU_OCOO:
                    tabTypeNm = "ocoo";
                    break;
                case Constant.SU_OPER:
                    tabTypeNm = "oper";
                    break;
                case Constant.SU_PLAN:
                    tabTypeNm = "plan";
                    break;
                case Constant.SU_ROUT:
                    tabTypeNm = "rout";
                    break;
                case Constant.SU_TOWN:
                    tabTypeNm = "town";
                    break;
                case Constant.SU_TOWR:
                    tabTypeNm = "towr";
                    break;
                case Constant.SU_TRAF:
                    tabTypeNm = "traf";
                    break;
                default:
                    Log2.e("\nAtRecordChng.AtRecordChange(): ERROR: unknown table type: " + tableType);
                    ErrMsg.UtPrintMessage(Error.GENERROR, "Unknown table type - Audit Record not saved");
                    return;
            }

            auditTrail.tabletype = tabTypeNm;
            auditTrail.filename = pdfName;

            if ((rc = UserInfo.UtGetUserInfo(out userStuff)) != Constant.SUCCESS)
            {
                ErrMsg.UtPrintMessage(rc);
                auditTrail.ultrixid = "<UNKNOWN>";
                auditTrail.micsid = "<UNKNOWN>";
            }
            else
            {
                auditTrail.ultrixid = userStuff.micsUser.ultrixid;
                auditTrail.micsid = userStuff.micsUser.micsid;
            }

            if (!Ssutil.IntTableExist(DynAuditTrail.TableName))
            {
                /* audit trail table doesn't exist */
                Log2.e("\nAtRecordChng.AtRecordChange(): ERROR: audit trail table does not exist: " + DynAuditTrail.TableName);
                Console.Write("Audit Trail table {0} not found - record not stored.", DynAuditTrail.TableName);
                Ssutil.DisConn(hConn);
                return;
            }

            DynAuditTrail.AuditTrailInsert(auditTrail, nullInds);

            // Create the full path names for the tempory results file and
            // the final audit results file.
            tmpListFile = tmpPath + pdfName + ".txt";

            auditPath = String.Format("{0}audit\\{1}\\{2}.{3}", GenUtil.UtGetFilesDir(), tabTypeNm,
                                        pdfName, GenUtil.FileNameTime(curDate, curTime));

            //...Log2.v("\n\nAtRecordChng.AtRecordChange(): tmpListFile: " + tmpListFile);
            //...Log2.v("\nAtRecordChng.AtRecordChange(): auditPath  : " + auditPath + "\n");

            // AH: the following code block is redundant.
            if (GenUtil.UtGetPgmName(tableType, Constant.PRINT_PGM, out printPgmNm) != Constant.SUCCESS)
            {
                Log2.e("\nAtRecordChng.AtRecordChange(): ERROR: Print program not defined");
                ErrMsg.UtPrintMessage(Error.GENMESS, "Print program not defined");
                return;
            }

            /* call print routine */
            if (tableType == Constant.FT)
            {
                /* parameter, as well as the pdf name */

                // We will now run the program FtPrint in a command shell.
                //
                // Usage:  FtPrint <dbname> <projectCode> [-o<outFilePath>] <printFlag> <tableName>
                // =====

                clArgs = String.Format("{0} {1} {2} {3}", Info.DbName, Info.ProjectCode, "L", Info.PdfName);

                cmd = Ssutil.GetBinPath("FtPrint", Info.DbName);

                WindowsShell.RunCommand(cmd, clArgs, out stdOutTxt, out stdErrTxt, out exitCode);

                // The results of the execution of FtPrint were written to stdOutTxt.
                // Now write these results to the temporary file.
                File.WriteAllText(tmpListFile, stdOutTxt);

                //...Log2.v("\n\nAtRecordChng.AtRecordChange(): audit file contents:\n" + stdOutTxt);

                if (exitCode != 0)
                {
                    Log2.e("\nAtRecordChng.AtRecordChange(): FT: ERROR: WindowsShell.RunCommand() failed for: " + cmd + " " + clArgs);
                    Log2.e("\nStdOut:\n" + stdOutTxt);
                    Log2.e("\nStdErr:\n" + stdErrTxt);
                }

            }
            else if (tableType == Constant.FE)
            {   /* Not a TS pdf - don't need the 'long' parameter */

                // We will now run the program EsPrint in a command shell.
                //
                // Usage:  EsPrint  <dbName>  <outDir>  <pdfName>  <projectCode> [Y]
                // =====
                // 
                // The outDir parameter *MUST* end with a '\' (e.g. d:\users\ahulme\temp\).
                // Y = Yes use Itanium format dates (?!)

                clArgs = String.Format("{0} {1} {2} {3}", Info.DbName, tmpPath, Info.PdfName, Info.ProjectCode);

                cmd = Ssutil.GetBinPath("EsPrint", Info.DbName);

                // EsPrint *always* writes its results to a file, not to Console.Out.
                // EsPrint's 2nd argument is the path of the directory to write the results file to.
                WindowsShell.RunCommand(cmd, clArgs, out stdOutTxt, out stdErrTxt, out exitCode);

                if (exitCode != 0)
                {
                    Log2.e("\nAtRecordChng.AtRecordChange(): ERROR: FE: WindowsShell.RunCommand() failed for: " + cmd + " " + clArgs);
                }

            }
            else
            {
                //	It is an sdf.

                // We will now run the program SdfPrint in a command shell.
                //
                // Usage:  SdfPrint.exe  dbName  outDir  sdfType pdfName  projectCode
                // =====

                clArgs = String.Format("{0} {1} {2} {3} {4}", Info.DbName, tmpPath, tabTypeNm, Info.SdfName, Info.ProjectCode);

                cmd = Ssutil.GetBinPath("SdfPrint", Info.DbName);

                WindowsShell.RunCommand(cmd, clArgs, out stdOutTxt, out stdErrTxt, out exitCode);

                if (exitCode != 0)
                {
                    Log2.e("\nAtRecordChng.AtRecordChange(): ERROR: SDF: WindowsShell.RunCommand() failed for: " + cmd + " " + clArgs);
                    Log2.e("\nstdOutTxt:\n" + stdOutTxt);
                    Log2.e("\nstdErrTxt:\n" + stdErrTxt);
                    Log2.e("\nexitCode = " + exitCode);
                }

            }

            try
            {
                // Copy the contents of the tempory file to the FCSA audit file.
                File.Copy(tmpListFile, auditPath);

                // Clean up.
                File.Delete(tmpListFile);
            }
            catch (Exception e)
            {
                Log2.e("\nAtRecordChng.AtRecordChange(): ERROR: call to File.WriteAllText() threw EXCEPTION: \n" + e.Message);
                string str = String.Format("\nAudit Trail: Could not copy input file {0}\nto audit directory:{1}", tmpListFile, auditPath);
                GenUtil.SetErr(str);
                Console.Write(str);
            }

            Console.Write("\r\nAudit Trail - Record has been made ({0}) of changes to the database\r\n", auditPath);

            return;
        }	/* ----- End of atRecordChng ----- */






    }
}
