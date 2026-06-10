# Documented File: Parameters.cs
**Repository Path:** `GetProfRep\Parameters.cs`
**Primary Layer:** `GetProfRep`
**Namespace:** `GetProfRep`

## Source Code Representation
```csharp
﻿using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetProfRep
{
    /// <summary>
    /// This class encapsulates the many command-line and intermediate
    /// variables that are used in the GetOHLrep Main() method and enables
    /// a more elegant partioning of the C# source code than was used in the
    /// legacy C/C++ code.
    /// </summary>
    public class Parameters
    {
        public string cHTMLsw;

        public string mSiteCallSignA = "";
        public string mSiteCallSignB = "";
        public string mSiteNameA = "";
        public string mSiteNameB = "";

        public Enums.eRepType eRep;

        public string mLat27A;
        public string mLong27A;
        public string mLat83A;
        public string mLong83A;

        public string mLat27B;
        public string mLong27B;
        public string mLat83B;
        public string mLong83B;

        /*	Store the lats and longs as seconds * 100 */
        public int nLata83;
        public int nLatb83;

        public int nLonga83;
        public int nLongb83;

        /*	UTM Coordinates */
        public short nutmzonea;
        public double utmae;
        public double utman;
        public short nutmzoneb;
        public double utmbe;
        public double utmbn;

        public double fbearinga = 0.0;
        public double fbearingb = 0.0;
        public double fdist = 0.0;
        public double fdistinc = 0.0;

        public string mMapsUsed;
    }
}

```
