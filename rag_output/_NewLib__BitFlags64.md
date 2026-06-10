# Documented File: BitFlags64.cs
**Repository Path:** `_NewLib\BitFlags64.cs`
**Primary Layer:** `_NewLib`
**Namespace:** `_NewLib`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    using _Configuration;
    using SQLLEN = Int64;

    /// <summary>
    /// This class encapsulates the data representation and functionality of 
    /// a bitmap as an ordered set of qty. 64 bit-fields (0 or 1) stored as a 
    /// single UInt64 integer; if the value of this integer is written
    /// out as 64 binary digits then 'bit zero' is the leftmost digit.
    /// </summary>

    public class BitFlags64
    {
        private UInt64 mBitFlags;
        private const int NUMBITS = 64;
        // Define the value correspoding to 0th bit of a 64-bit integer, numbered from the left.
        //                                0123456789012345
        private const UInt64 BIT_ZERO = 0x8000000000000000;

        public const UInt64 ALL_BITS_SET = 0xFFFFFFFFFFFFFFFF;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public BitFlags64()
        {
            mBitFlags = 0;
        }

        /// <summary>
        /// Creates an instance of a bitmap object with 64 bit-fields and a prescribed
        /// initial UInt64 value.
        /// </summary>
        /// <param name=""> - prescribed initial UInt64 value.</param>
        /// <returns></returns>
        public BitFlags64(UInt64 bitFlags)
        {
            mBitFlags = bitFlags;
        }

        /// <summary>
        /// This method returns the underlying UInt64 representation of the bitmap.
        /// </summary>
        public UInt64 Value
        {
            get { return mBitFlags; }
        }

        /// <summary>
        /// This method validates that a bitPosition value lies with
        /// the range [0, 63].
        /// </summary>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        private static bool IsValidBitPosition(int bitPosition)
        {
            return (bitPosition >= 0) && (bitPosition < NUMBITS);
        }

        /// <summary>
        /// This method sets (to unity) a bit at a prescribed position in the bitmap.
        /// </summary>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        public bool SetBit(int bitPosition)
        {
            bool retVal = false;

            // Check that bitPosition is valid.
            if (IsValidBitPosition(bitPosition))
            {
                // Create the bit-mask for the prescribed position.
                UInt64 mask = BIT_ZERO >> bitPosition;
                // Set the bit at the prescribed position by ORing mBitFlags with the mask.
                mBitFlags |= mask;
                retVal = true;
            }

            return retVal;
        }

        /// <summary>
        /// This method returns true if a bit at a prescribed position in the bitmap
        /// has the value 1.
        /// </summary>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        public bool TestBit(int bitPosition)
        {
            bool retVal = false;

            // Check that bitPosition is valid.
            if (IsValidBitPosition(bitPosition))
            {
                // Create the bit-mask for the prescribed position.
                UInt64 mask = BIT_ZERO >> bitPosition;
                // Test the bit at the prescribed position by ANDing mBitFlags with the mask.
                if ((mBitFlags & mask) > 0)
                {
                    retVal = true; ;
                }
            }

            return retVal;
        }

        /// <summary>
        /// This method sets all 64 bits in the bitmap to the value 1.
        /// </summary>
        /// <param name=""></param>
        public void SetAllBits()
        {
            //            0123456789012345
            mBitFlags = 0xFFFFFFFFFFFFFFFF;
        }

        /// <summary>
        /// This method returns an UInt64 integer value that has a bit
        /// at a prescribed position set to 1 and all other bits set to 0.
        /// This integer value can then be used as a 'mask' for subsequent bitwise AND and OR
        /// operations.
        /// </summary>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        public static UInt64 GetMask(int bitPosition)
        {
            // Create the bit-mask for the prescribed position.
            return BIT_ZERO >> bitPosition;
        }

        /// <summary>
        /// This method performs a bitwise AND operation on all 64 bits of the bitmap
        /// using the prescribed mask as the second operand.
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public BitFlags64 BitWiseAND(BitFlags64 mask)
        {
            ulong maskValue = mask.Value;

            ulong resultAND = mBitFlags & maskValue;

            return new BitFlags64(resultAND);
        }

        /// <summary>
        /// This method performs a bitwise OR operation on all 64 bits of the bitmap
        /// using the prescribed mask as the second operand.
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public BitFlags64 BitWiseOR(BitFlags64 mask)
        {
            ulong maskValue = mask.Value;

            ulong resultOR = mBitFlags | maskValue;

            return new BitFlags64(resultOR);
        }

        /// <summary>
        /// This method returns true if the current bitmap has
        /// all 64 bits as zero (no bits set).
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public bool HasNoBitsSet()
        {
            return mBitFlags == 0;
        }

        /// <summary>
        /// This method returns true if the current bitmap has 
        /// any bit set to 1.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public bool HasBitsSet()
        {
            return mBitFlags != 0;
        }

        /// <summary>
        /// This method return a string giving the value of the
        /// underlying UInt64 integer in hexadecimal format.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("0x{0:x16}", mBitFlags);
        }



    }
}

```
