using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// This class encapsulates the notion of a dyad of strings (e.g. A/B ) in which order is not significant,
    /// i.e. A/B is the same dyad as B/A.
    /// </summary>
    public class NonOrderedPair
    {
        private string mStrA;
        private string mStrB;

        public string A
        {
            get { return mStrA; }
            set { mStrA = value; }
        }
        public string B
        {
            get { return mStrB; }
            set { mStrB = value; }
        }

        /// <summary>
        /// The default constructor is intentionally hidden as private.
        /// </summary>
        private NonOrderedPair() { }

        /// <summary>
        /// This constructor creates a NonOrderedPair object given two prescribed string values.
        /// </summary>
        /// <param name="strA"></param>
        /// <param name="strB"></param>
        public NonOrderedPair(string strA, string strB)
        {
            mStrA = strA;
            mStrB = strB;
        }

        /// <summary>
        /// This method returns a single line string providing the two member values of this NonOrderedPair object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}, {1}", mStrA, mStrB);
        }

        /// <summary>
        /// This method returns true if this NonOrderedPair object is equal to a prescribed NonOrderedPair object; else false.
        /// </summary>
        /// <param name="testpair"></param>
        /// <returns></returns>
        public bool Equals(NonOrderedPair testpair)
        {
            bool parallelEqual = mStrA.Equals(testpair.A) && mStrB.Equals(testpair.B);
            bool diagonalEqual = mStrA.Equals(testpair.B) && mStrB.Equals(testpair.A);

            return parallelEqual || diagonalEqual;
        }

        /// <summary>
        /// This method returns true if a prescribed NonOrderedPair object is equal to an element in a prescribed list
        /// of NonOrderedPair objects; else false.
        /// </summary>
        /// <param name="nonOrderedPair"></param>
        /// <param name="nonOrderedPairList"></param>
        /// <returns></returns>
        public static bool IsInList(NonOrderedPair nonOrderedPair, List<NonOrderedPair> nonOrderedPairList)
        {
            bool isInList = false;

            foreach (NonOrderedPair nop in nonOrderedPairList)
            {
                if (nop.Equals(nonOrderedPair))
                {
                    isInList = true;
                    break;
                }
            }

            return isInList;
        }

        /// <summary>
        /// This method returns true if this NonOrderedPair object is equal to an element in a prescribed list
        /// of NonOrderedPair objects; else false.
        /// </summary>
        /// <param name="nonOrderedPairList"></param>
        /// <returns></returns>
        public bool IsInList(List<NonOrderedPair> nonOrderedPairList)
        {
            bool isInList = false;

            foreach (NonOrderedPair nop in nonOrderedPairList)
            {
                if (nop.Equals(this))
                {
                    isInList = true;
                    break;
                }
            }

            return isInList;
        }


    }
}
