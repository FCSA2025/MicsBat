# Documented File: MasterRecord.cs
**Repository Path:** `ImportAnte\MasterRecord.cs`
**Primary Layer:** `ImportAnte`
**Namespace:** `ImportAnte`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportAnte
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLLEN = Int64;
    public class MasterRecord
    {
        private static string msgBuf;
        private static int rc;


        public static void ParseFirstLine(int lineNum, string[] fields, out SuAnte anteStruct, out SQLLEN[] anteNulls, ref short importOK)
        {
            // 'out' requirement.
            anteStruct = new SuAnte();
            anteNulls = NullHelper.CreateArrayOfNullInd(SuAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            /* antenna record */
            if ((rc = CSV.ParseFieldAsString(fields, 1, out anteStruct.cmd, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante cmd field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante cmd field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.CMD] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.CMD] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsString(fields, 2, out anteStruct.recstat, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante recstat field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante recstat field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.RECSTAT] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.RECSTAT] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsString(fields, 3, out anteStruct.acode, 12)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante acode field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante acode field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante acode field, MUST be present for import.\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
                anteNulls[SuAnte.ACODE] = Constant.DB_NOT_NULL;
            }
            else
            {
                anteNulls[SuAnte.ACODE] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsInt(fields, 4, out anteStruct.axtype)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0},import Ante axtype field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante axtype field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.AXTYPE] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.AXTYPE] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsString(fields, 5, out anteStruct.axref, 12)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante axref field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante axref field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.AXREF] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.AXREF] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsFloatRound(fields, 6, out anteStruct.again, -1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante again field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante again field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.AGAIN] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.AGAIN] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsFloatRound(fields, 7, out anteStruct.abw, -1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante abw field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante abw field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.ABW] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.ABW] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsShort(fields, 8, out anteStruct.arms)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante arms field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante arms field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.ARMS] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.ARMS] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsString(fields, 9, out anteStruct.aband, 10)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante aband field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante aband field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.ABAND] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.ABAND] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsString(fields, 10, out anteStruct.amanu, 10)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante amanu field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante amanu field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.AMANU] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.AMANU] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsString(fields, 11, out anteStruct.apattern, 12)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante apattern field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante apattern field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.APATTERN] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.APATTERN] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsString(fields, 12, out anteStruct.amodel, 15)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante amodel field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante amodel field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.AMODEL] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.AMODEL] = Constant.DB_NOT_NULL;
            }

        }

        public static void ParseSecondLine(int lineNum, string[] fields, ref SuAnte anteStruct, ref SQLLEN[] anteNulls, ref short importOK)
        {
            /* antenna record */
            string secondcmd;
            if ((rc = CSV.ParseFieldAsString(fields, 1, out secondcmd, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante cmd field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante cmd field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                if (!secondcmd.Equals(anteStruct.cmd))
                {
                    msgBuf = String.Format("Error - line #{0}, conflicting master record command\n", lineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
            }

            if ((rc = CSV.ParseFieldAsShort(fields, 2, out anteStruct.anip)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante anip field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante anip field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.ANIP] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.ANIP] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsFloatRound(fields, 3, out anteStruct.ax0, -1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante ax0 field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante ax0 field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.AX0] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.AX0] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsString(fields, 4, out anteStruct.adesc, 20))
                == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante adesc field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante adesc field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.ADESC] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.ADESC] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsString(fields, 5, out anteStruct.antype, 8)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante antype field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante antype field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.ANTYPE] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.ANTYPE] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsFloatRound(fields, 6, out anteStruct.aftbr, -1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante aftbr field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante aftbr field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.AFTBR] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.AFTBR] = Constant.DB_NOT_NULL;
            }

            /* TASK 1061: New code. */
            if ((rc = CSV.ParseFieldAsDoubleRound(fields, 7, out anteStruct.lofreq, -1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante lofreq field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante lofreq field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.LOFREQ] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.LOFREQ] = Constant.DB_NOT_NULL;
            }

            if ((rc = CSV.ParseFieldAsDoubleRound(fields, 8, out anteStruct.hifreq, -1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante hifreq field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante hifreq field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.HIFREQ] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.HIFREQ] = Constant.DB_NOT_NULL;
            }

            /* Task 1061a: New code */
            string dummy;
            if (CSV.ParseFieldAsString(fields, 9, out dummy, 1) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, import Ante master; extra fields ignored\r\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
            }
        }

        public static void ParseThirdLine(int lineNum, string[] fields, ref SuAnte anteStruct, ref SQLLEN[] anteNulls, ref short importOK)
        {
            /* antenna record */
            string secondcmd;
            if ((rc = CSV.ParseFieldAsString(fields, 1, out secondcmd, 1)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante cmd field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante cmd field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                if (!secondcmd.Equals(anteStruct.cmd))
                {
                    msgBuf = String.Format("Error - line #{0}, conflicting master record command\n", lineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                }
            }
            /* Task 1061a: End new code */

            if ((rc = CSV.ParseFieldAsString(fields, 2, out anteStruct.bandcodes, 99))
                    == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante bandcodes field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante bandcodes field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.BANDCODES] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.BANDCODES] = Constant.DB_NOT_NULL;
            }
            /* TASK 1061: End new code. */

            if ((rc = CSV.ParseFieldAsDate(fields, 3, out anteStruct.mdate, 25)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante mdate field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante mdate field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.MDATE] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.MDATE] = Constant.DB_NOT_NULL;
                bool ok = ValUtil.UtValidateDate(anteStruct.mdate);
                //if (rc == Constant.FAILED_FORMAT || rc == Constant.FAILED_RANGE)
                if (!ok)
                {
                    msgBuf = String.Format("Error - line #{0}, importAnte mdate field, invalid date\n", lineNum);
                    ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                    importOK = Constant.FAILURE;
                }
            }

            if ((rc = CSV.ParseFieldAsString(fields, 4, out anteStruct.mtime, 6)) == Constant.UT_INV_CONV)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante mtime field, invalid conversion\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == Constant.UT_EOLN)
            {
                msgBuf = String.Format("Error - line #{0}, import Ante mtime field, end of data found\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
                importOK = Constant.FAILURE;
            }
            else if (rc == 0)
            {
                anteNulls[SuAnte.MTIME] = Constant.DB_NULL;
            }
            else
            {
                anteNulls[SuAnte.MTIME] = Constant.DB_NOT_NULL;
            }

            string dummy;
            if (CSV.ParseFieldAsString(fields, 5, out dummy, 1) != Constant.UT_EOLN)
            {
                msgBuf = String.Format("Warning - line #{0}, import Ante master; extra fields ignored\r\n", lineNum);
                ErrMsg.UtPrintMessage(Error.GENERROR, msgBuf);
            }
        }



    }
}

```
