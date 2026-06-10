# Documented File: FtUtil.cs
**Repository Path:** `FtValidate\FtUtil.cs`
**Primary Layer:** `FtValidate`
**Namespace:** `FtValidate`

## Source Code Representation
```csharp
using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtValidate
{
    /// <summary>
    /// Provides miscellaneous 'utility' methods used by FtValidate.
    /// </summary>
    public class FtUtil
    {
        /// <summary>
        /// This method returns true if the prescribed command letter is one of A, B, D, N or U;
        /// otherwise false.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static bool IsValidCmd(string cmd)
        {
            return (cmd != null && cmd.Length > 0 && Strings.CharIsOneOfABDNU(cmd[0]));
        }

        /// <summary>
        /// This method returns true if the prescribed command letter is one of A, B, D, N or U;
        /// otherwise false; an output message is also provided that identifies if the command
        /// letter is blank or invalid.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool IsValidCmd(string cmd, out string message)
        {
            // 'out' requirement.
            message = "";

            bool isValidCmd = IsValidCmd(cmd);

            if (!isValidCmd)
            {
                if (cmd == null)
                {
                    message = "Invalid Command (null); must be A, B, D, N or U.";
                }
                else if (cmd.Length == 0)
                {
                    message = "Missing Command; must be A, B, D, N or U.";
                }
                else
                {
                    message = String.Format("Invalid Command ({0}); must be A, B, D, N or U.", cmd);
                }
            }

            return isValidCmd;
        }

        /// <summary>
        /// This method inputs 'vh' and 'set' and then determines the channel's polarization.
        /// See page B-72 of TSIP reference Guide.
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="set"></param>
        /// <param name="pol"></param>
        public static void FtGetPol(short vh, int set, out string pol)
        {
            if (vh <= 0 || vh > 4 || set <= 0 || set > 4)
            {
                /* Invalid combination. Should be impossible to get here! */
                pol = " ";
            }
            else
            {
                pol = Constant.Polarity[vh - 1, set - 1].ToString();
            }
        }



    }
}

```
