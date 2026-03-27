using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _NewLib.Enums;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates the notion of a pair (Tx/Rx) of HiLo values; it is used
    /// by the static class HiLoCheckSuppRedesigned which re-implements the functionality
    /// of the legacy class HiLoCheckSupp using lists. 
    /// </summary>
    public class HiLoPair
    {
        public HiLo txSense;
        public HiLo rxSense;

        /// <summary>
        /// Default constructor - initializes the HiLo pair to NotSet / NotSet.
        /// </summary>
        public HiLoPair()
        {
            NotSet();
        }

        /// <summary>
        /// Constructor - sets the object's (Tx/Rx) HiLo values to those prescribed.
        /// </summary>
        /// <param name="txSense"></param>
        /// <param name="rxSense"></param>
        public HiLoPair(HiLo txSense, HiLo rxSense)
        {
            this.txSense = txSense;
            this.rxSense = rxSense;
        }

        /// <summary>
        /// Constructor - sets the both of object's (Tx/Rx) HiLo values to that prescribed.
        /// </summary>
        /// <param name="txRxSense"></param>
        public HiLoPair(HiLoPair txRxSense)
        {
            this.txSense = txRxSense.txSense;
            this.rxSense = txRxSense.rxSense;
        }

        /// <summary>
        /// This method returns a formatted string that provides the current values of 
        /// this object's HiLo sense pair.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0,9} / {1,9}", txSense, rxSense);
        }

        /// <summary>
        /// This method sets this object's (Tx/Rx) HiLo values to NotSet.
        /// </summary>
        public void NotSet()
        {
            txSense = HiLo.NotSet;
            rxSense = HiLo.NotSet;
        }

        /// <summary>
        /// This method returns true if neither the Tx nor Rx value is HiLo.Violation; otherwise false.
        /// </summary>
        /// <returns></returns>
        public bool HasNoViolations()
        {
            return (txSense != Enums.HiLo.Violation) && (rxSense != Enums.HiLo.Violation);
        }

        /// <summary>
        /// This method returns true if both the Tx and Rx values are HiLo.Violation; otherwise false.
        /// </summary>
        /// <returns></returns>
        public bool BothAreViolations()
        {
            return ((txSense == Enums.HiLo.Violation) && (rxSense == Enums.HiLo.Violation));
        }

        /// <summary>
        /// This method returns true if either the Tx or Rx value is HiLo.Violation; otherwise false.
        /// </summary>
        /// <returns></returns>
        public bool HasViolation()
        {
            return (txSense == Enums.HiLo.Violation) || (rxSense == Enums.HiLo.Violation);
        }


    }
}
