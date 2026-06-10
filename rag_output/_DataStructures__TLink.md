# Documented File: TLink.cs
**Repository Path:** `_DataStructures\TLink.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates the dataset required to characterize a single link. 
    /// Multi-hop links can be characterized as arrays of TLink objects.
    /// </summary>
    public class TLink
    {
        public Enums.DB eWhich;  //0
        public string call2;  //1
        public string bndcde;  //2
        public int nNumAnts;  //3
        public int nNumChan;  //4
        public int[] aAnts;  //5
        public int[] aChan;  //6

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public TLink()
        {

        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public TLink(TLink_OLD tLink_OLD)
        {
            // Scalars.
            nNumAnts = tLink_OLD.nNumAnts;
            nNumChan = tLink_OLD.nNumChan;

            // Enum.
            eWhich = (Enums.DB)tLink_OLD.eWhich;

            // Strings.
            call2 = tLink_OLD.call2;
            bndcde = tLink_OLD.bndcde;

            // Arrays.
            aAnts = new int[nNumAnts];
            Marshal.Copy(tLink_OLD.aAntsPtr, aAnts, 0, nNumAnts);

            aChan = new int[nNumChan];
            Marshal.Copy(tLink_OLD.aChanPtr, aChan, 0, nNumChan);
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

            sb.Append("\n\n===== TLink object =====");
            sb.Append("\neWhich = " + eWhich);
            sb.Append("\ncall2 = " + call2);
            sb.Append("\nbndcde = " + bndcde);
            sb.Append("\nnNumAnts = " + nNumAnts);
            for (int i = 0; i < nNumAnts; i++)
            {
                sb.Append("\n     aAnts[" + i + "] = " + aAnts[i]);
            }
            sb.Append("\nnNumChan = " + nNumChan);
            for (int i = 0; i < nNumChan; i++)
            {
                sb.Append("\n     aChan[" + i + "] = " + aChan[i]);
            }

            return sb.ToString();
        }







    }
}

```
