using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    /// <summary>
    /// Provides a method that determines whether a PDF has been validated, or not.
    /// </summary>
    public class FilewUtil
    {
#if PINVOKE
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int utFilewValidated(int i, string s1, string s2);
#endif

        //-------------------------------------------------------------------------------------

        /// <summary>
        /// Determines whether an existing PDF has been validated, or not.
        /// </summary>
        /// <param name="tabType"> - table type: either Constant.FE or Constant.FT</param>
        /// <param name="tableName"> - displayable table name.</param>
        /// <param name="validatedFor"> - validation code.</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - validatedFor contains a validation code.</para>
        /// <para>- any other value - validatedFor contains an empty string.</para>
        public static int UtFilewValidated(
                        int tabType,                /* Table type */
                        string tableName,            /* Displayable name */
                        out string validatedFor)    /* validation code */
        {
            //...Log2.v("\n\nFilewUtil.UtFilewValidated(): Entry");
            Log2.v("\ntableName: " + tableName);
            int rc = -666; ;

            FtTitl pftTitle;
            FeTitl pfeTitle;

            validatedFor = "";
            switch (tabType)
            {
                case Constant.FT:
                    rc = FtUtils.FtGetTitle(out pftTitle, tableName);
                    if (rc == Constant.SUCCESS)
                    {
                        validatedFor = pftTitle.validated;
                    }
                    break;

                case Constant.FE:
                    rc = FeUtils.FeGetTitle(out pfeTitle, tableName);
                    if (rc == Constant.SUCCESS)
                    {
                        validatedFor = pfeTitle.validated;
                    }
                    break;

                default:
                    //...Log2.v("\r\nFilewUtil.UtFilewValidated(): case default");
                    validatedFor = "N";
                    rc = Constant.SUCCESS;
                    break;
            }

            //...Log2.v("\n\nFilewUtil.UtFilewValidated(): Exit, returns " + rc + " validatedFor = " + validatedFor);
            return (rc);
        }	/* ***** End of utFilewValidated ***** */

    }
}
