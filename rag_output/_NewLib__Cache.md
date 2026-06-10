# Documented File: Cache.cs
**Repository Path:** `_NewLib\Cache.cs`
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

    /// <summary>
    /// This abstract class encapsulates the representation, storage and 
    /// functionality of a cache of objects of the same type.
    /// </summary>
    /// <remarks>
    /// A cache is a software component that stores data so that future requests
    /// for that data can be served faster.
    /// <para>The data stored in a cache might be 
    /// the result of an earlier computation, or the duplicate of data stored 
    /// elsewhere.</para>
    /// <para>
    /// A cache 'hit' occurs when the requested data can be found in a 
    /// cache, while a cache 'miss' occurs when it cannot.</para>
    /// <para>
    /// Cache hits are served by 
    /// reading data from the cache, which is faster than recomputing a result 
    /// or reading from a slower data store; thus, the more requests that can be served
    /// from the cache, the faster the system performs.</para>
    /// </remarks>
    /// <typeparam name="T"> - caller-prescribed type.</typeparam>
    public abstract class Cache<T>
    {
        private static T[] mT;
        private static int[] mWhen;
        private static int mMaxSize;
        private static int mNext = 0;
        private static int mNow = 0;
        private static int mCalls = 0;
        private static int mHits = 0;

        /// <summary>
        /// This method returns the maximum number of objects that can
        /// be stored in the cache.
        /// </summary>
        public int MaxSize
        {
            get { return mMaxSize; }
        }

        /// <summary>
        /// This method returns the number of times the cache has been
        /// interrogated via a get request.
        /// </summary>
        public int Calls
        {
            get { return mCalls; }
        }

        /// <summary>
        /// This method returns the number of times that a cache 'hit' has occurred.
        /// </summary>
        public int Hits
        {
            get { return mHits; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private Cache()
        {
        }

        /// <summary>
        /// This constructor prescribes the maximum number of objects that
        /// can be stored in the cache.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public Cache(int maxSize)
        {
            mMaxSize = maxSize;

            mT = new T[mMaxSize];

            mWhen = new int[mMaxSize]; // Initially filled with all zeroes.
        }

        /// <summary>
        /// This method interrogates the cache for a 'hit' matching the prescribed
        /// 'key'. If a hit is found this method returns a 'safe copy' of the found object; if 
        /// we have a cache 'miss' then the default value of the type T is returned. 
        /// (Note: all reference types have a default value of null.)
        /// </summary>
        /// <param name="key"> - T object to be matched with all objects currently in the cache.</param>
        /// <returns></returns>
        public T Get(T key)
        {
            mCalls++;

            T tFound = default(T);  // Returns null for reference types.

            for (int i = 0; i < mNext; i++)
            {
                if (Hit(mT[i], key))
                {
                    tFound = mT[i];
                    mHits++;
                    mNow++;
                    mWhen[i] = mNow;
                    break;
                }
            }

            return SafeCopy(tFound);
        }

        /// <summary>
        /// This method inserts an object of type T into the cache. If all 'slots'
        /// in the cache are occupied then the object stored in the 
        /// Least Recently Used (LRU) slot is abandoned and the new value stored
        /// in its place.
        /// </summary>
        /// <param name="t"></param>
        public void Put(T t)
        {
            int slot = 0;

            // Check whether we have an available slot in the cache; if so use it.
            // If not, we need to recycle the LeastRecently Used (LRU) slot.
            if (mNext < mMaxSize)
            {
                slot = mNext;
                mNext++;
            }
            else // Find the LRU slot and reuse it.
            {
                int slotLRU = 0;
                int whenLRU = mWhen[0];
                for (int i = 0; i < mMaxSize; i++)
                {
                    if (mWhen[i] < whenLRU)
                    {
                        slotLRU = i;
                        whenLRU = mWhen[slotLRU];
                    }
                }
                slot = slotLRU;
            }

            // We might need to make and cache deep-copies of mutable objects.
            mT[slot] = SafeCopy(t);

            mNow++;
            mWhen[slot] = mNow;
        }

        /// <summary>
        /// The caller must provide an override of this method that
        /// defines when a 'hit' occurs between two objects of type T.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public abstract bool Hit(T t1, T t2);

        /// <summary>
        /// The caller must provide an override of this method that
        /// decides if, and how, an object of type T needs to be
        /// deep-cloned before/after writing/reading to/from the cache.
        /// This is needed to prevent inadvertent corruption of data in the cache.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public abstract T SafeCopy(T t);

        /// <summary>
        /// This method returns the 'hit rate' as a percentage of all get requests.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public int GetHitRatePC()
        {
            int pc = 0;

            if (mCalls > 0)
            {
                pc = (int)((100.0 * mHits / mCalls) + 0.5);
            }
            else
            {
                pc = 0;
            }

            return pc;
        }

        /// <summary>
        /// This method returns the current internal 'state' parameters of the cache.
        /// </summary>
        /// <param name="nCalls"></param>
        /// <param name="nHits"></param>
        /// <param name="nSize"></param>
        /// <param name="nUsed"></param>
        public void GetStats(out int nCalls, out int nHits, out int nSize, out int nUsed)
        {
            nCalls = mCalls;
            nHits = mHits;
            nSize = mMaxSize;
            nUsed = mNext;
        }

    }
}

```
