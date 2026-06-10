# Documented File: EmissionDesignator.cs
**Repository Path:** `ComsearchToTAFL\EmissionDesignator.cs`
**Primary Layer:** `ComsearchToTAFL`
**Namespace:** `ComsearchToTAFL`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using _NewLib;

namespace ComsearchToTAFL
{
    /// <summary>
    /// This class provides methods that extract the bandwidth from an ITU emissions designator string. 
    /// </summary>
    public class EmissionDesignator
    {
        public enum BwPattern { _000A, _00A0, _0A00, INVALID };

        /// <summary>
        /// This method returns bandwidth, in Hz, from an ITU emissions designator string. 
        /// </summary>
        /// <param name="emissionDesignator"></param>
        /// <returns></returns>
        public static double GetBandwidthHz(string emissionDesignator)
        {
            double bwHz = double.MinValue;

            // If the emissionDesignator is NULL then return a negative value.
            if (emissionDesignator == null) return bwHz;

            // Tidy up the emissionDesignator string to ensure it has a canonical format.
            emissionDesignator = emissionDesignator.Trim().ToUpper();

            // Emission Designators must have at least 7 characters.
            if (emissionDesignator.Length < 7) return bwHz;

            // Emission Designators must have no more than 9 characters.
            if (emissionDesignator.Length > 9) return bwHz;

            // Identify the bandwidth field pattern.
            string bwField = emissionDesignator.Substring(0, 4);
            BwPattern bwPattern = BwPattern.INVALID;

            if (Regex.IsMatch(bwField, @"\d\d\d[A-Z]"))
            {
                bwPattern = BwPattern._000A;
            }
            else if (Regex.IsMatch(bwField, @"\d\d[A-Z]\d"))
            {
                bwPattern = BwPattern._00A0;
            }
            else if (Regex.IsMatch(bwField, @"\d[A-Z]\d\d"))
            {
                bwPattern = BwPattern._0A00;
            }

            if (bwPattern == BwPattern.INVALID) return bwHz;

            double multiplier = 1;
            double baseNumber = 0;
            char multiplierChar = '?';

            try
            {
                switch (bwPattern)
                {
                    case BwPattern._000A:
                        baseNumber = Convert.ToDouble(bwField.Substring(0, 3));
                        multiplierChar = bwField[3];
                        break;
                    case BwPattern._00A0:
                        baseNumber = Convert.ToDouble(bwField.Substring(0, 2) + bwField.Substring(3, 1));
                        baseNumber *= 0.1;
                        multiplierChar = bwField[2];
                        break;
                    case BwPattern._0A00:
                        baseNumber = Convert.ToDouble(bwField.Substring(0, 1) + bwField.Substring(2, 2));
                        baseNumber *= 0.01;
                        multiplierChar = bwField[1];
                        break;
                    default:
                        return bwHz;
                }
            }
            catch (Exception e)
            {
                Log2.e("\n\nEmission.Designator.GetBandwidthHz(): ERROR: exception: {0}\n{1}", e.Message, e.StackTrace);
                return bwHz;
            }

            // Decode the multiplier character.
            switch (multiplierChar)
            {
                case 'H':
                    multiplier = 1;
                    break;
                case 'K':
                    multiplier = 1000;
                    break;
                case 'M':
                    multiplier = 1000000;
                    break;
                case 'G':
                    multiplier = 1000000000;
                    break;
                default:
                    return bwHz;
            }

            bwHz = baseNumber * multiplier;

            return bwHz;
        }

        /// <summary>
        /// This method returns bandwidth, in KHz, from an ITU emissions designator string. 
        /// </summary>
        /// <param name="emissionDesignator"></param>
        /// <returns></returns>
        public static double GetBandwidthKHz(string emissionDesignator)
        {
            return 0.001 * GetBandwidthHz(emissionDesignator);
        }







    }
}

```
