using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _NewLib
{
    /// <summary>
    /// This class provides methods that allow threads to communicate with each other by signaling
    /// using .NET EventWaitHandle thread synchronization events; this signalling can be among threads
    /// for the same process and/or between multiple processes.
    /// </summary>
    /// <remarks>
    /// The EventWaitHandle class provides access to named system synchronization events.
    /// <para>
    /// The .NET EventWaitHandle class allows threads to communicate with each other by signaling. 
    /// Typically, one or more threads block on an EventWaitHandle until an unblocked thread calls the 
    /// Set method, releasing one or more of the blocked threads. A thread can signal an 
    /// EventWaitHandle and then block on it, by calling the static WaitHandle.SignalAndWait method.
    /// </para><para>
    /// The behavior of an EventWaitHandle that has been signaled depends on its reset mode. 
    /// An EventWaitHandle created with the EventResetMode.AutoReset flag resets automatically 
    /// when signaled, after releasing a single waiting thread. An EventWaitHandle created with 
    /// the EventResetMode.ManualReset flag remains signaled until its Reset method is called.
    /// </para><para>
    /// Automatic reset events provide exclusive access to a resource. If an automatic reset 
    /// event is signaled when no threads are waiting, it remains signaled until a thread 
    /// attempts to wait on it. The event releases the thread and immediately resets, blocking 
    /// subsequent threads.
    /// </para><para>
    /// Manual reset events are like gates. When the event is not signaled, threads that wait on 
    /// it will block. When the event is signaled, all waiting threads are released, and the 
    /// event remains signaled (that is, subsequent waits do not block) until its Reset method 
    /// is called. Manual reset events are useful when one thread must complete an activity 
    /// before other threads can proceed.
    /// </para><para>
    /// See:  https://docs.microsoft.com/en-us/dotnet/api/system.threading.eventwaithandle?view=netframework-4.7.2</para>
    /// </remarks>
    public class NamedEvent
    {
        /// <summary>
        /// This is the default constructor declared as 'private' to make it inaccessible
        /// to external callers.
        /// </summary>
        private NamedEvent() { }


        /// <summary>
        /// Opens the specified named synchronization event, if it already exists; if not extant
        /// the method creates a new EventWaitHandle object.
        /// </summary>
        /// <param name="name"> - the name of the system synchronization event to open.</param>
        /// <param name="initialState"> - 'true' sets the initial state to signaled if the named event is created as a result of this call; 'false' to set it to nonsignaled.</param>
        /// <param name="mode"> - prescribes whether the event resets automatically or manually.</param>
        /// <returns></returns>
        public static EventWaitHandle OpenOrCreate(string name, bool initialState, EventResetMode mode)
        {
            EventWaitHandle ewh = null;

            try
            {
                ewh = EventWaitHandle.OpenExisting(name);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                //Handle does not exist, create it.
                ewh = new EventWaitHandle(initialState, mode, name);
            }

            return ewh;

        }

        /// <summary>
        /// Opens the specified named synchronization event, if it already exists.
        /// </summary>
        /// <param name="name"> - the name of the system synchronization event to open.</param>
        /// <returns> - ab EventWaitHandle object.</returns>
        public static EventWaitHandle OpenOrWait(string name)
        {
            EventWaitHandle ewh = null;

            while (null == ewh)
            {
                try
                {
                    ewh = EventWaitHandle.OpenExisting(name);
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    Thread.Sleep(1000);
                    //...Log2.v("\nNamedEvent.OpenOrWait(): tick..." );
                }
            }

            return ewh;

        }
    }
}
