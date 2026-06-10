# Documented File: PatternStruct.cs
**Repository Path:** `_DataStructures\PatternStruct.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates the dataset for an antenna pattern.
    /// </summary>
    public class PatternStruct
    {
        public string acode = "";
        public int numPts;
        public int useCounter;
        public short[] nulls = new short[5];  //[5];
        public float[,] pattern = new float[Constant.MAX_ANT_PTS, 5];

        /// <summary>
        /// This method returns a 'deep copy' of this object's current field values.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public PatternStruct DeepCopy()
        {

            PatternStruct newPS = new PatternStruct();

            newPS.acode = acode;
            newPS.numPts = numPts;
            newPS.useCounter = useCounter;

            for (int i = 0; i < 5; i++)
            {
                newPS.nulls[i] = nulls[i];
            }

            for (int i = 0; i < Constant.MAX_ANT_PTS; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    newPS.pattern[i, j] = pattern[i, j];
                }
            }

            return newPS;
        }





    }
}

```
