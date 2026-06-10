# Documented File: ChanHiLo.cs
**Repository Path:** `_DataStructures\ChanHiLo.cs`
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
    public class ChanHiLo
    {
        public string cmd;
        public string call1;
        public string call2;
        public string bndcde;
        public string chid;
        public double freqtx;
        public double freqrx;
        public HiLoSensePair hiLoSensePair;

        private ChanHiLo() { }

        public ChanHiLo(string cmd, string call1, string call2, string bndcde, string chid, double freqtx, double freqrx)
        {
            this.cmd = cmd;
            this.call1 = call1;
            this.call2 = call2;
            this.bndcde = bndcde;
            this.chid = chid;
            this.freqtx = freqtx;
            this.freqrx = freqrx;
            this.hiLoSensePair = new HiLoSensePair();
        }

        public ChanHiLo(FtChan ftChan)
        {
            cmd = ftChan.cmd;
            call1 = ftChan.call1;
            call2 = ftChan.call2;
            bndcde = ftChan.bndcde;
            chid = ftChan.chid;
            freqtx = ftChan.freqtx;
            freqrx = ftChan.freqrx;
            hiLoSensePair = new HiLoSensePair();
        }

        public ChanHiLo(MtChan mtChan)
        {
            cmd = "-";
            call1 = mtChan.call1;
            call2 = mtChan.call2;
            bndcde = mtChan.bndcde;
            chid = mtChan.chid;
            freqtx = mtChan.freqtx;
            freqrx = mtChan.freqrx;
            hiLoSensePair = new HiLoSensePair();
        }

        /// <summary>
        /// This method returns a formatted single-line string providing the values of this object's members,
        /// including the HiLoSensePair value but excluding the cmd field.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0,9}, {1,9}, {2,4}, {3,4}, {4,9:F0} / {5,9:F0}   {6}", call1, call2, bndcde, chid, freqtx, freqrx, hiLoSensePair.ToString());
        }

        /// <summary>
        /// This method returns a formatted single-line string providing the values of this object's members,
        /// excluding the HiLoSensePair value but excluding the cmd field.
        /// </summary>
        /// <returns></returns>
        public string ToStringNoHiLo()
        {
            return String.Format("{0,9}, {1,9}, {2,4}, {3,4}, {4,9:F0} / {5,9:F0}", call1, call2, bndcde, chid, freqtx, freqrx);
        }

        /// <summary>
        /// This method returns a formatted single-line string providing the values of this object's members,
        /// including the HiLoSensePair value but excluding the cmd field.
        /// </summary>
        /// <returns></returns>
        public string ToStringAll()
        {
            return String.Format("{0,1}, {1,9}, {2,9}, {3,4}, {4,4}, {5,9:F0} / {6,9:F0}   {7}", cmd, call1, call2, bndcde, chid, freqtx, freqrx, hiLoSensePair.ToString());
        }

        /// <summary>
        /// This method creates a list of ChanHiLo objects from a list of FtChan objects.
        /// </summary>
        /// <param name="ftChanList"> - the prescribed list of FtChan objects.</param>
        /// <returns></returns>
        public static List<ChanHiLo> ConvertList(List<FtChan> ftChanList)
        {
            List<ChanHiLo> chanHiLoList = new List<ChanHiLo>();

            foreach (FtChan ftChan in ftChanList) chanHiLoList.Add(new ChanHiLo(ftChan));

            return chanHiLoList;
        }

        /// <summary>
        /// This method creates a list of ChanHiLo objects from a list of MtChan objects.
        /// </summary>
        /// <param name="mtChanList"> - the prescribed list of MtChan objects.</param>
        /// <returns></returns>
        public static List<ChanHiLo> ConvertList(List<MtChan> mtChanList)
        {
            List<ChanHiLo> chanHiLoList = new List<ChanHiLo>();

            foreach (MtChan mtChan in mtChanList) chanHiLoList.Add(new ChanHiLo(mtChan));

            return chanHiLoList;
        }

        /// <summary>
        /// This method returns a sub-list of ChanHiLo objects selected from a prescribed list of
        /// ChanHiLo objects where the bndcde field is equal to a prescribed bndcde string value,
        /// </summary>
        /// <param name="chanHiLoList"> - the prescribed list of ChanHiLo objects.</param>
        /// <param name="bndcde"> - the bndcde to be selected.</param>
        /// <returns></returns>
        public static List<ChanHiLo> GetSubListForBndcde(List<ChanHiLo> chanHiLoList, string bndcde)
        {
            List<ChanHiLo> subList = new List<ChanHiLo>();

            foreach (ChanHiLo chanHiLo in chanHiLoList)
            {
                if (chanHiLo.bndcde.Equals(bndcde)) subList.Add(chanHiLo);
            }

            return subList;
        }

        /// <summary>
        /// This method returns the total number of distinct sites identified in the call1
        /// fields of a list of ChanHiLo objects; distinct mean no duplicates.
        /// </summary>
        /// <param name="chanHiLoList"> - the prescribed list of ChanHiLo objects.</param>
        /// <returns></returns>
        public static int CountSites(List<ChanHiLo> chanHiLoList)
        {
            List<string> siteList = new List<string>();
            List<string> resultList = new List<string>();

            foreach (ChanHiLo chanHiLo in chanHiLoList)
            {
                siteList.Add(chanHiLo.call1);
            }

            // Remove duplicates.
            foreach (string str in siteList)
            {
                if (!resultList.Contains(str))
                {
                    resultList.Add(str);
                }
            }

            return resultList.Count;
        }

        /// <summary>
        /// This method returns a string that can be used as a unique key for this ChanHiLo
        /// object; the search key is simply the concatanation of the string values of this object's
        /// call1, call2, bndcde and chid values.
        /// </summary>
        /// <returns></returns>
        public string UniqueKey()
        {
            return (call1.Trim() + call2.Trim() + bndcde.Trim() + chid.Trim()).ToUpper();
        }




    }
}

```
