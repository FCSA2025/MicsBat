using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetProfRep
{
    /// <summary>
    /// This class provides methods that perform specific, lower-level, fragmentary 
    /// writing operations in support of the generation of PLAIN, HTML and/or CSV
    /// output reports.
    /// </summary>
    public class Write
    {
        /// <summary>
        /// This method writes the "<html> and <head>" tags at the beginning of an HTML report
        /// and "</body> and </html>" closing tags at the end of the report.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="onoroff"></param>
        public static void htm(Enums.eRepType e, int onoroff)
        {
            if (e == Enums.eRepType.eHTML)
            {
                if (onoroff == Constant.ON)
                {
                    // In the legacy C/C++ code, __FILE__ is a standard predefined macro that expands 
                    // to the name of the current source file, in the form of a C string constant. 
                    // This is the path by which the preprocessor opened the file, not the short name 
                    // specified in ‘#include’ or as the input file name argument. For example, 
                    // "/usr/local/include/myheader.h" is a possible expansion of this macro.
                    Console.Write("<html>\r\n<head>\r\n<title>HTML from {0}</title>\r\n</head>\r\n<body>",
                            Info.__FILE__());
                }
                else
                {
                    Console.Write("\r\n</body></html>");
                }
            }
        }


        /// <summary>
        /// This method writes HTML <h1></h1> type header tags for a
        /// prescribed level of heading.
        /// </summary>
        /// <param name="nLevel"></param>
        /// <param name="e"></param>
        /// <param name="onoroff"></param>
        public static void h(int nLevel, Enums.eRepType e, int onoroff)
        {
            if (e == Enums.eRepType.eHTML)
            {
                if (onoroff == Constant.ON)
                {
                    Console.Write("\r\n<h{0}>", nLevel);
                }
                else
                {
                    Console.Write("\r\n</h{0}>", nLevel);
                }
            }
        }

        /// <summary>
        /// This method writes the HTML tags required to create a table
        /// having rows and columns.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="onoroff"></param>
        public static void table(Enums.eRepType e, int onoroff)
        {
            if (e == Enums.eRepType.eHTML)
            {
                if (onoroff == Constant.ON)
                {
                    Console.Write("\r\n<table><tr><td>");
                }
                else
                {
                    Console.Write("\r\n</td></tr></table>");
                }
            }
        }

        /// <summary>
        /// This method writes the HTML tag pair "</td><td>"
        /// </summary>
        /// <param name="e"></param>
        public static void td(Enums.eRepType e)
        {
            if (e == Enums.eRepType.eHTML)
            {
                Console.Write("\r\n</td><td>");
            }
        }

        /// <summary>
        /// This method writes the HTML tags "</td><tr><td>".
        /// </summary>
        /// <param name="e"></param>
        public static void tr(Enums.eRepType e)
        {
            if (e == Enums.eRepType.eHTML)
            {
                Console.Write("</td>\r\n<tr><td>");
            }
        }

        /// <summary>
        /// This method writes a PLAIN text line break or an
        /// HTML "<br>" tag.
        /// </summary>
        /// <param name="e"></param>
        public static void br(Enums.eRepType e)
        {
            if (e == Enums.eRepType.eHTML)
            {
                Console.Write("\r\n<br>");
            }
            else
            {
                Console.Write("\r\n");
            }
        }



    }
}

