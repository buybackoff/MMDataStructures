using System;
using ST = System.Threading;

namespace MMDataStructures
{

    // TODO adjust locks for exlusive write/read-only modes

    /// <summary>
    /// Named mutex
    /// </summary>
    public class Mutex : IDisposable
    {
        private ST.Mutex StMutex { get; set; }

        /// <summary>
        /// Mutex was created in this constructor
        /// </summary>
        public bool Created { get; set; }

        /// <summary>
        /// Creates or gets named mutex that is not initially owned
        /// </summary>
        /// <param name="name"></param>
        public Mutex(string name)
        {
            bool created;
            StMutex = new ST.Mutex(false, name, out created);
            Created = created;
        }
        /// <summary>
        /// Blocks the current thread until the current WaitHandle receives a signal. (Inherited from WaitHandle.)
        /// </summary>
        /// <returns></returns>
        public bool WaitOne() { return StMutex.WaitOne(); }

        /// <summary>
        /// Blocks the current thread until the current WaitHandle receives a signal, using a 32-bit signed integer to specify the time interval.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Timeout.Infinite (-1) to wait indefinitely. If millisecondsTimeout is zero, the method does not block. It tests the state of the wait handle and returns immediately.</param>
        /// <returns>true if the current instance receives a signal; otherwise, false.</returns>
        public bool WaitOne(int millisecondsTimeout) { return StMutex.WaitOne(millisecondsTimeout); }

        /// <summary>
        /// Releases the Mutex once.
        /// </summary>
        public void ReleaseMutex() { StMutex.ReleaseMutex(); }

        /// <summary>
        /// Dispose mutex if it was created by us
        /// </summary>
        public void Dispose()
        {
            if (this.StMutex != null && Created) {
                this.StMutex.Dispose();
            }
        }
    }
}
