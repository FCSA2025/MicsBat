# Documented File: GridStruct.cs
**Repository Path:** `_DataStructures\GridStruct.cs`
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

    /// <summary>
    /// Objects of this class are used to read geographical grid data from the 
    /// data files located at "D:\dted50\data" and "D:\dted250\data"; 
    /// see <see cref="_Auxlib.AxNTv2Lib.ReadGrid()"/>
    /// </summary>
    public class GridStruct
    {
        public string subname;
        public string parent;
        public float latmin;
        public float latmax;
        public float latinterval;
        public float longmin;
        public float longmax;
        public float longinterval;
        public int offset;
        public int pointcount;
        public int gridcount;
        public GridStruct[] subgrids;

        //---------------------------------------------------------------------

        public const int NUM_COLUMNS = 12;

        //---------------------------------------------------------------------

        /// <summary>
        /// The default constructor.
        /// </summary>
        public GridStruct()
        {
        }

        /// <summary>
        /// This method returns a string that
        /// provides the current values of the internal field values as a single line of text.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string ToStringAsLines()
        {
            string thisLine = "";
            string nextLines = "";

            thisLine = String.Format("\n{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}",
                                        subname, parent, latmin, latmax, latinterval, longmin, longmax,
                                        longinterval, offset, pointcount, gridcount);
            if (subgrids == null)
            {
                thisLine += " <null>";
            }
            else
            {
                thisLine += " <instance>";
                for (int i = 0; i < gridcount; i++)
                {
                    // Beware: this is recursive!
                    nextLines += subgrids[i].ToStringAsLines();
                }
            }

            return thisLine + nextLines;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the internal field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\nsubname =      " + subname);
            sb.Append("\nparent =       " + parent);
            sb.Append("\nlatmin =       " + latmin);
            sb.Append("\nlatmax =       " + latmax);
            sb.Append("\nlatinterval =  " + latinterval);
            sb.Append("\nlongmin =      " + longmin);
            sb.Append("\nlongmax =      " + longmax);
            sb.Append("\nlonginterval = " + longinterval);
            sb.Append("\noffset =       " + offset);
            sb.Append("\npointcount =   " + pointcount);
            sb.Append("\ngridcount =    " + gridcount);

            if (subgrids == null)
            {
                sb.Append("\nsubgrids =    null");
            }
            else
            {
                for (int i = 0; i < gridcount; i++)
                {
                    if (subgrids[i] == null)
                    {
                        sb.Append("\n===== subgrids[" + i + "] = null");
                    }
                    else
                    {
                        sb.Append("\n===== subgrids[" + i + "] =\n" + subgrids[i].ToString());
                    }

                }
            }

            return sb.ToString();
        }



    }
}

```
