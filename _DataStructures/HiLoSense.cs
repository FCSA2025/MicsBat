using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates the notion of a HiLo Sense as being one of 'Hi', 
    /// 'Lo' or 'NotSet'.
    /// </summary>
    public class HiLoSense
    {
        public enum Sense {Hi, Lo, NotSet};

        public const Sense HI = Sense.Hi;
        public const Sense LO = Sense.Lo;
        public const Sense NOTSET = Sense.NotSet;

        private Sense mValue;

        public Sense Value { get { return mValue; } set { mValue = value; } }

        /// <summary>
        /// Default constructor - initializes this object's HiLo sense value to NotSet.
        /// </summary>
        public HiLoSense()
        {
            mValue = Sense.NotSet;
        }

        /// <summary>
        /// Constructor - initializes this object's HiLo sense value to that prescribed.
        /// </summary>
        /// <param name="sense"></param>
        public HiLoSense(Sense sense)
        {
            mValue = sense;
        }

        /// <summary>
        /// This method returns a string that provides the current value of this object's HiLo sense.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = null;

            switch (mValue)
            {
                case Sense.Hi:
                    str = "Hi";
                    break;
                case Sense.Lo:
                    str = "Lo";
                    break;
                case Sense.NotSet:
                    str = "--";
                    break;
            } // switch

            return str;
        }

        /// <summary>
        /// This method returns true if this object's HiLoSense value is the same as a prescribed HiLoSense value.
        /// </summary>
        /// <param name="hiLoSense"></param>
        /// <returns></returns>
        public bool Equals(HiLoSense hiLoSense) { return this.mValue == hiLoSense.Value; }


        /// <summary>
        /// This method returns true if this object's HiLoSense value is 'Hi'.
        /// </summary>
        /// <returns></returns>
        public bool IsHi() { return mValue == Sense.Hi; }

        /// <summary>
        /// This method returns true if this object's HiLoSense value is 'Lo'.
        /// </summary>
        /// <returns></returns>
        public bool IsLo() { return mValue == Sense.Lo; }

        /// <summary>
        /// This method returns true if this object's HiLoSense value is compatible
        /// with a prescribed HiLo sense value.
        /// </summary>
        /// <returns></returns>
        public bool IsCompatibleWith(HiLoSense testHiLoSense)
        {
            bool isCompatible = false;

            if (mValue == Sense.NotSet)
            {
                isCompatible = true;
            }
            else if ((mValue == Sense.Hi) && (testHiLoSense.Value != Sense.Lo))
            {
                isCompatible = true;
            }
            else if ((mValue == Sense.Lo) && (testHiLoSense.Value != Sense.Hi))
            {
                isCompatible = true;
            }

            return isCompatible;
        }


    }
}
