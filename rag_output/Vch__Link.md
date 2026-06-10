# Documented File: Link.cs
**Repository Path:** `Vch\Link.cs`
**Primary Layer:** `Vch`
**Namespace:** `Vch`

## Source Code Representation
```csharp
﻿using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vch
{
    /// <summary>
    /// This class is key to the operation of the Vch program; an object of this class 
    /// holds information for one link from a local site; links are grouped by 
    /// band code, and contain the indices of the antennas and channels in the FtSiteStr 
    /// for its local site. 
    /// </summary>
    public class Link
    {
        public string call2;
        public string bndcde;
        public int nNumAnts;
        public List<int> aAnts;
        public int nHighestTXAnt;
        public int nHighestRXAnt;
        public int nNumChans;
        public List<int> aChans;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public Link()
        {
            call2 = "";
            bndcde = "";

            aAnts = new List<int>();
            aChans = new List<int>();
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

            sb.Append("\n\n===== T_Link object =====");
            sb.Append("\ncall2 = " + call2);
            sb.Append("\nbndcde = " + bndcde);
            sb.Append("\nnNumAnts = " + nNumAnts);

            for (int i = 0; i < nNumAnts; i++)
            {
                sb.Append("\n     aAnts[" + i + "] = " + aAnts[i]);
            }

            sb.Append("\nnHighestTXAnt = " + nHighestTXAnt);
            sb.Append("\nnHighestRXAnt = " + nHighestRXAnt);

            sb.Append("\nnNumChans = " + nNumChans);

            for (int i = 0; i < nNumChans; i++)
            {
                sb.Append("\n     aChans[" + i + "] = " + aChans[i]);
            }

            return sb.ToString();
        }





    }
}

```
