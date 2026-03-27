using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    public class SuOpCode
    {
        public string ultrixid;
        public string oper;

        //--------------------------------------------------------------------------

        public const int ULTRIXID = 0;
        public const int OPER = 1;

        public const int ULTRIXID_SZ = Constant.USERID_SZ;
        public const int OPER_SZ = Constant.OPER_SZ;

        //--------------------------------------------------------------------------

        /// <summary>
        /// This is the public default constructor of object of this class.
        /// </summary>
        public SuOpCode()
        {
            ultrixid = "";
            oper = "";
        }

        /// <summary>
        /// This method returns a string containing the values of the members
        /// ultrixid and oper of 'this' object with annotation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("ultrixid = {0} ; oper = {1}", ultrixid, oper);
        }









    }
}
