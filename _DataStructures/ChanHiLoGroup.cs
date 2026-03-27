using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    public class ChanHiLoGroup
    {
        public enum HiLoGroupState { OK, Violation }

        public const HiLoGroupState OK = HiLoGroupState.OK;
        public const HiLoGroupState VIOLATION = HiLoGroupState.Violation;

        public string bndcde;
        public List<ChanHiLo> chanHiLoList;
        public HiLoSensePair hiLoGroupSensePair;
        public HiLoGroupState hiLoGroupState;
        public string message;
        public bool bndcdeInCommon;
        public HiLoSensePair referenceSensePair;

        public ChanHiLoGroup() { }

        public ChanHiLoGroup(string bndcde) : this(bndcde, new List<ChanHiLo>())
        {
        }

        public ChanHiLoGroup(string bndcde, List<ChanHiLo> chanHiLoList)
        {
            this.bndcde = bndcde;
            this.chanHiLoList = chanHiLoList;
            hiLoGroupSensePair = new HiLoSensePair();
            hiLoGroupState = new HiLoGroupState();
            message = "";
            bndcdeInCommon = false;
            referenceSensePair = new HiLoSensePair();
        }

        /// <summary>
        /// This method returns a string providing this ChanHiLoGroup object's state value and,
        /// if the state is OK, the group HiLoSense Pair value.
        /// </summary>
        /// <returns></returns>
        public string GroupToString()
        {
            string str = hiLoGroupState.ToString();

            if (hiLoGroupState == HiLoGroupState.OK)
            {
                str += String.Format("  {0}", hiLoGroupSensePair);
            }

            return str;
        }

        /// <summary>
        /// This method returns an annotated multi-line string providing the member values of 
        /// this ChanHiLoGroup object's list of ChanHiLo objects.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            int count = 0;
            foreach (ChanHiLo chanHiLo in chanHiLoList)
            {
                sb.Append(String.Format("\n#{0}:   {1}", ++count, chanHiLo.ToString()));
            }

            return sb.ToString();
        }

        public bool IsInViolation() { return hiLoGroupState == HiLoGroupState.Violation; }

        public void SetViolation() { hiLoGroupState = HiLoGroupState.Violation; }

















    }
}
