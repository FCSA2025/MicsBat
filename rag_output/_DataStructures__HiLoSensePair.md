# Documented File: HiLoSensePair.cs
**Repository Path:** `_DataStructures\HiLoSensePair.cs`
**Primary Layer:** `_DataStructures`
**Namespace:** `_DataStructures`

## Source Code Representation
```csharp
﻿using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _NewLib.Enums;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates the notion of a HiLo sense pair as a dyad comprising
    /// two individual HiLo sense values corresponding to the (Tx, Rx) frequency pair
    /// in a FtChan or MtChan object.
    /// </summary>
    public class HiLoSensePair
    {
        public HiLoSense txSense;
        public HiLoSense rxSense;

        /// <summary>
        /// This is the default constructor.
        /// </summary>
        public HiLoSensePair()
        {
            txSense = new HiLoSense(HiLoSense.Sense.NotSet);
            rxSense = new HiLoSense(HiLoSense.Sense.NotSet);
        }

        /// <summary>
        /// This constructor creates a new HiLoSensePair object from a prescribed pair
        /// of HiLoSense objects corresponding to a (Tx, Rx) dyad.
        /// </summary>
        /// <param name="txSense"></param>
        /// <param name="rxSense"></param>
        public HiLoSensePair(HiLoSense txSense, HiLoSense rxSense)
        {
            this.txSense = txSense;
            this.rxSense = rxSense;
        }

        /// <summary>
        /// This constructor creates a HiLoSensePair object whose txSense and rxSense members
        /// equal those of a prescribed HiLoSensePair value.
        /// </summary>
        /// <param name="txRxSense"></param>
        public HiLoSensePair(HiLoSensePair txRxSense)
        {
            this.txSense = txRxSense.txSense;
            this.rxSense = txRxSense.rxSense;
        }

        /// <summary>
        /// This method returns true if this object's txSense value equals HiLoSense.LO and
        /// its rxSense value also equals HiLoSense.LO; otherwise false.
        /// </summary>
        /// <returns></returns>
        public bool IsLoLo()
        {
            return (txSense.IsLo()) && (rxSense.IsLo());
        }

        /// <summary>
        /// This method returns true if this object's txSense value equals HiLoSense.HI and
        /// its rxSense value also equals HiLoSense.HI; otherwise false.
        /// </summary>
        /// <returns></returns>
        public bool IsHiHi()
        {
            return (txSense.IsHi()) && (rxSense.IsHi());
        }

        /// <summary>
        /// This method returns true if this object's txSense value is not opposite
        /// to a prescribed HiLoSensePair objects's rxSense value; otherwise false.
        /// </summary>
        /// <param name="testHiLoSensePair"></param>
        /// <returns></returns>
        public bool TxRxInterSiteConflict(HiLoSensePair testHiLoSensePair)
        {
            bool isHiLo = (txSense.IsHi()) && (testHiLoSensePair.rxSense.IsHi());
            bool isLoHi = (txSense.IsLo()) && (testHiLoSensePair.rxSense.IsLo());

            return isHiLo || isLoHi;
        }

        /// <summary>
        /// This method returns true if this object's rxSense value is not opposite
        /// to a prescribed HiLoSensePair objects's txSense value; otherwise false.
        /// </summary>
        /// <param name="testHiLoSensePair"></param>
        /// <returns></returns>
        public bool RxTxInterSiteConflict(HiLoSensePair testHiLoSensePair)
        {
            bool isHiLo = (rxSense.IsHi()) && (testHiLoSensePair.txSense.IsHi());
            bool isLoHi = (rxSense.IsLo()) && (testHiLoSensePair.txSense.IsLo());

            return isHiLo || isLoHi;
        }

        /// <summary>
        /// This method reterns a formatted string providing this object's txSense and
        /// rxSense member values using the dyad format "A / B".
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("({0,2} / {1,-2})", txSense.ToString(), rxSense.ToString());
        }

        /// <summary>
        /// This method returns a string corresponding to a prescribed HiLoSense object's value member, 
        /// one of "Hi", "Lo", or "--" meaning NotSet.
        /// </summary>
        /// <param name="hiLoSense"> - the prescribed HiLoSense object.</param>
        /// <returns></returns>
        public static string SenseToToken(HiLoSense hiLoSense)
        {
            string token = "--";
            switch (hiLoSense.Value)
            {
                case HiLoSense.HI:
                    token = "Hi";
                    break;
                case HiLoSense.LO:
                    token = "Lo";
                    break;
            }

            return token;
        }

        public string TxSenseToToken() { return SenseToToken(txSense); }

        public string RxSenseToToken() { return SenseToToken(rxSense); }

        /// <summary>
        /// This method return true if this object is not opposite to a prescribed HiLoSensePair object; 
        /// otherwise false.
        /// </summary>
        /// <param name="testPair"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public bool IsNotOppositeTo(HiLoSensePair testPair, out string fault)
        {
            // 'out' requirement.
            fault = "";

            bool txIsNotOpposite = false;
            bool rxIsNotOpposite = false;

            // Tx analysis.
            switch (txSense.Value)
            {
                case HiLoSense.LO:
                    if (testPair.txSense.Value == HiLoSense.LO) { txIsNotOpposite = true; fault += "TX"; }
                    break;
                case HiLoSense.HI:
                    if (testPair.txSense.Value == HiLoSense.HI) { txIsNotOpposite = true; fault += "TX"; }
                    break;
            }

            // Rx analysis.
            switch (rxSense.Value)
            {
                case HiLoSense.LO:
                    if (testPair.rxSense.Value == HiLoSense.LO) { rxIsNotOpposite = true; fault += "RX"; }
                    break;
                case HiLoSense.HI:
                    if (testPair.rxSense.Value == HiLoSense.HI) { rxIsNotOpposite = true; fault += "RX"; }
                    break;
            }

            return txIsNotOpposite || rxIsNotOpposite;
        }

        /// <summary>
        /// This method returns true if a prescribed HiLoSensePair object is compatible
        /// with this current object.
        /// </summary>
        /// <param name="testPair"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public bool IsCompatibleWith(HiLoSensePair testPair, out string fault)
        {
            // 'out' requirement.
            fault = "";

            bool txIsCompatibleWith = true;
            bool rxIsCompatibleWith = true;

            // Tx analysis.
            switch (txSense.Value)
            {
                case HiLoSense.NOTSET:
                    // Any sense is compatible with NotSet.
                    break;
                case HiLoSense.LO:
                    // The only sense that is not compatible is Hi.
                    if (testPair.txSense.Value == HiLoSense.HI) { txIsCompatibleWith = false; }
                    break;
                case HiLoSense.HI:
                    // The only sense that is not compatible is Lo.
                    if (testPair.txSense.Value == HiLoSense.LO) { txIsCompatibleWith = false; }
                    break;
            }

            // Rx analysis.
            switch (rxSense.Value)
            {
                case HiLoSense.NOTSET:
                    // Any sense is compatible with NotSet.
                    break;
                case HiLoSense.LO:
                    // The only sense that is not compatible is Hi.
                    if (testPair.rxSense.Value == HiLoSense.HI) { rxIsCompatibleWith = false; }
                    break;
                case HiLoSense.HI:
                    // The only sense that is not compatible is Lo.
                    if (testPair.rxSense.Value == HiLoSense.LO) { rxIsCompatibleWith = false; }
                    break;
            }

            if ( !txIsCompatibleWith && !rxIsCompatibleWith)
            {
                fault = "   VIOLATION (Tx and Rx)";
            }
            else if (!txIsCompatibleWith)
            {
                fault = "   VIOLATION (Tx)";
            }
            else if (!rxIsCompatibleWith)
            {
                fault = "   VIOLATION (Rx)";
            }

            //Console.Write("\n{0},   {1},   {2},   {3}", this.ToString(), testPair.ToString(), txIsCompatibleWith, rxIsCompatibleWith);

            return txIsCompatibleWith && rxIsCompatibleWith;
        }

    }
}

```
