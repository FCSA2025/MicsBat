using _Configuration;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _DataStructures
{
    /// <summary>
    /// This class encapsulates a bit map comprising 256 bits; the bits are
    /// enumerated from left-to-right with the first bit at position 0.
    /// </summary>
    public class BandBits
    {
        public uint[] bitArray;
        public const int LENGTH = 8;

        //-----------------------------------------------------------------

        private const int BITSPERINT = 8 * sizeof(uint);
        public const int MAXNUMBITS = BITSPERINT * LENGTH;

        //-----------------------------------------------------------------

        /// <summary>
        /// Default constructor; creates the bitmap and sets all bits to zero.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public BandBits()
        {
            bitArray = new uint[LENGTH];
            Initialize();
        }

        /// <summary>
        /// Constructor: sets the bitmap i.a.w. FtSite object's bandwords.
        /// </summary>
        /// <param name="ftSite"></param>
        public BandBits(FtSite ftSite)
        {
            bitArray = new uint[LENGTH];

            bitArray[0] = (uint)ftSite.bandwd1;
            bitArray[1] = (uint)ftSite.bandwd2;
            bitArray[2] = (uint)ftSite.bandwd3;
            bitArray[3] = (uint)ftSite.bandwd4;
            bitArray[4] = (uint)ftSite.bandwd5;
            bitArray[5] = (uint)ftSite.bandwd6;
            bitArray[6] = (uint)ftSite.bandwd7;
            bitArray[7] = (uint)ftSite.bandwd8;
        }

        /// <summary>
        /// Constructor: sets the bitmap i.a.w. prescribed uint[] array
        /// that MUST have 8 elements.
        /// </summary>
        /// <param name="uintArray">uint array with 8 elements that holds a 256-bit field.</param>
        public BandBits(uint[] uintArray)
        {
            if ((uintArray == null) || (uintArray.Length !=8))
            {
                Log2.e("\n\nBandBits.BandBits(): ERROR: uintArray is either NULL or it does not have 8 elements.");
                Application.ExitQuietly(666);
            }

            bitArray = new uint[LENGTH];

            bitArray[0] = uintArray[0];
            bitArray[1] = uintArray[1];
            bitArray[2] = uintArray[2];
            bitArray[3] = uintArray[3];
            bitArray[4] = uintArray[4];
            bitArray[5] = uintArray[5];
            bitArray[6] = uintArray[6];
            bitArray[7] = uintArray[7];
        }

        /// <summary>
        /// This method sets all qty. 256 bits to zero.
        /// </summary>
        /// <param name=""></param>
        public void Initialize()
        {
            ClearAllBits();
        }

        /// <summary>
        /// This method returns a string that lists the values of each of the
        /// qty. 8 bandwords as an 8-digit hex number.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < Constant.FT_BANDWD_CT; i++)
            {
                string str = String.Format("\nbandBits[{0}] = 0x{1:x8}", i, bitArray[i]);

                sb.Append(str);
            }

            return sb.ToString();
        }

        /// <summary>
        /// This static method returns a BandBits object that is the result of bit-wise
        /// ORing two prescribed BandBits objects.
        /// </summary>
        /// <param name="bandBits1"></param>
        /// <param name="bandBits2"></param>
        /// <returns></returns>
        public static BandBits BitWiseOR(BandBits bandBits1, BandBits bandBits2)
        {
            BandBits result = new BandBits();

            for (int i = 0; i < LENGTH; i++)
            {
                result.bitArray[i] = bandBits1.bitArray[i] | bandBits2.bitArray[i];
            }

            return result;
        }

        /// <summary>
        /// This method modifies 'this' object by applying a bit-wise OR with the bits of a prescribed 
        /// BandBits object.
        /// </summary>
        /// <param name="bandBits1"></param>
        public void BitWiseOR(BandBits bandBits1)
        {
            for (int i = 0; i < LENGTH; i++)
            {
                bitArray[i] |= bandBits1.bitArray[i];
            }
        }

        /// <summary>
        /// This method clears (to zero) all of the bits of 'this' object.
        /// </summary>
        public void ClearAllBits()
        {
            for (int i = 0; i < LENGTH; i++)
            {
                bitArray[i] = 0;
            }
        }

        /// <summary>
        /// This method returns true if 'this' object has the bit at a prescribed bit-position
        /// is set; otherwise false.
        /// </summary>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        public bool CheckBit(int bitPosition)
        {
            if (bitPosition > MAXNUMBITS || bitPosition < 0)
            {
                string str = String.Format("BandBits.CheckBit(): ERROR: attempt to check a bit position that is out of range: bitPos = {0}, MAXNUMBITS = {1}", bitPosition, MAXNUMBITS);
                Log2.e("\n\n" + str);
                Application.Exit(str, 666);
            }

            /* step through the array of integers until we get to the right one */
            int index = 0;
            int bitPosInWord = bitPosition;
    
            while (bitPosInWord >= BITSPERINT)
            {
                index++;
                bitPosInWord = bitPosInWord - BITSPERINT;
            }

            if ((bitArray[index] & (1 << (BITSPERINT - bitPosInWord - 1))) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This method sets the prescribed bit-position to 1.
        /// </summary>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        public void SetBit(int bitPosition)
        {
            if (bitPosition > MAXNUMBITS || bitPosition < 0)
            {
                string str = String.Format("BandBits.SetBit(): ERROR: attempt to check a bit position that is out of range: bitPos = {0}, MAXNUMBITS = {1}", bitPosition, MAXNUMBITS);
                Log2.e("\n\n" + str);
                Application.Exit(str, 666);
            }

            /* step through the array of integers until we get to the right one */
            int index = 0;
            int bitPosInWord = bitPosition;

            while (bitPosInWord >= BITSPERINT)
            {
                index++;
                bitPosInWord = bitPosInWord - BITSPERINT;
            }

            bitArray[index] =  bitArray[index] | (uint)((1 << (BITSPERINT - bitPosInWord - 1)));
        }

        /// <summary>
        /// This methods returns a string of length 64 characters corresponding to the 
        /// binary representation of the first 64-bits of this BitBand object.
        /// of this BandBits object.
        /// </summary>
        /// <returns></returns>
        public string First64BitsAsBinary()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 64; i++)
            {
                string bitStr = "";

                if (CheckBit(i))
                {
                    bitStr = "1";
                }
                else
                {
                    bitStr = "0";
                }

                sb.Append(bitStr);
            }

            return sb.ToString();
        }

        /// <summary>
        /// This methods returns a string of length 64 characters corresponding to the 
        /// binary representation of the first 64-bits of this BitBand object.
        /// of this BandBits object; the string of binary digits is preceeded by
        /// a 'ruler' so that bit locations can be visually determined.
        /// </summary>
        /// <returns></returns>
        public string First64BitsAsBinaryWithRuler()
        {
            return String.Format("{0}\n{1}", Ruler64Bits(), First64BitsAsBinary()) ;
        }

        /// <summary>
        /// This method returns a string providing a 'ruler' that enables
        /// the index of a bit in a 64-bit binary string to be visually enumerated
        /// so that it can be manually looked-up in the MDB table main.sd_band.
        /// </summary>
        /// <returns></returns>
        public static string Ruler64Bits()
        {
            string str = "";

            str += "         1         2         3         4         5         6\n";
            str += "1234567890123456789012345678901234567890123456789012345678901234";

            return str;
        }


    }
}
