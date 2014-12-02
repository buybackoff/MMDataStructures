using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using MMDataStructures.DictionaryBacking;

namespace MMDataStructures {
    /// <summary>
    /// Array represent an array and stores it on disk using Memory Mapped Files
    /// instead of keeping the data in process memory. Memory Mapped Files will use the OS'
    /// functions for optimal caching of the data, yielding a reasonable tradeoff between
    /// speed and large amounts of data.
    /// 
    /// .Net applications will typically give random out-of-memory exceptions when approaching
    /// ~800mb data structures, specially if you need to keep several copies of an instance at
    /// a time. The problem is less frequent on 64bit systems than on 32bit, but still there.
    /// 
    /// This class will only accept value types and structs (which is a value type) since those
    /// objects always will take up the same amount of space. But make sure the struct contains
    /// only value types, or defined length strings.
    /// 
    /// For structs we are using this conversion:
    /// http://stackoverflow.com/questions/3278827/how-to-convert-a-structure-to-a-byte-array-in-c
    /// </summary>
    public class Array<T> : IEnumerable<T>, IDisposable
        where T : struct {
        private int _dataSize;
        internal FileManager Fm;
        private int _version;

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public PersistenceMode PersistenceMode { get { return Fm.PersistenceMode; } }

        /// <summary>
        /// Allow array to automatically grow if you access an indexer larger than the starting size
        /// </summary>
        public bool AutoGrow { get; set; }

        /// <summary>
        /// Return the number of elements in the array
        /// </summary>
        public virtual long Length { // for arrays, length = capacity, same as in CLR
            get { return Capacity; }
        }

        /// <summary>
        /// 
        /// </summary>
        private long Capacity { get { return Fm.Capacity / _dataSize; } }

        /// <summary>
        /// Set the position before setting or getting data
        /// </summary>
        //[Obsolete("Work with accessor")]
        //public long Position { get; set; }

        #endregion

        /// <summary>
        /// Create a new memory mapped array
        /// </summary>
        /// <param name="capacity">The number of elements to allocate in the array</param>
        /// <param name="fileName">File name of the MMF (relative to the DataPath)</param>
        /// <param name="autoGrow">Decide if the array can expand or not</param>
        /// <param name="persistenceMode"></param>
        public Array(long capacity, string fileName, bool autoGrow = false, PersistenceMode persistenceMode = PersistenceMode.TemporaryPersist) {
            switch (persistenceMode) {
                case PersistenceMode.Persist:
                case PersistenceMode.TemporaryPersist:
                    fileName = Path.Combine(Config.DataPath, fileName);
                    break;
                case PersistenceMode.Ephemeral:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("persistenceMode");
            }

            _dataSize = Marshal.SizeOf(typeof(T));
            AutoGrow = autoGrow;
            Fm = new FileManager(fileName, capacity * _dataSize, persistenceMode);
        }

        public T this[long index] {
            get {
                if (index < 0) {
                    throw new IndexOutOfRangeException("Tried to access item outside the array boundaries");
                }
                try {
                    Fm.FileMutex.WaitOne();
                    if (index >= Capacity) {
                        if (AutoGrow) {
                            Fm.EnsureCapacity((index + 1) * _dataSize);
                        } else {
                            throw new IndexOutOfRangeException("Tried to access item outside the array boundaries");
                        }
                    }

                    using (var va = Fm.CreateViewWrap()) {
                        //var buffer = new byte[_dataSize];
                        //var c = va.Va.UnReadArray(index * _dataSize, buffer, 0, _dataSize);
                        var buffer = va.VA.UnsafeReadBytes(index*_dataSize, _dataSize);
                        //Trace.Assert(c == _dataSize);
                        return Config.Serializer.Deserialize<T>(buffer);
                    }
                } finally {
                    Fm.FileMutex.ReleaseMutex();
                }
            }
            set {
                if (index < 0) {
                    throw new IndexOutOfRangeException("Tried to access item outside the array boundaries");
                }
                try {
                    Fm.FileMutex.WaitOne();
                    if (index >= Capacity) {
                        if (AutoGrow) {
                            Fm.EnsureCapacity((index + 1) * _dataSize);
                        } else {
                            throw new IndexOutOfRangeException("Tried to access item outside the array boundaries");
                        }
                    }
                    using (var va = Fm.CreateViewWrap()) {
                        var buffer = Config.Serializer.Serialize(value);
                        Fm.EnsureCapacity(index * _dataSize + buffer.Length);
                        va.VA.UnsafeWriteBytes(index * _dataSize, buffer);
                        _version++;
                    }
                } finally {
                    Fm.FileMutex.ReleaseMutex();
                }
            }
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator() {
            var currentVersion = _version;
            using (var va = Fm.Mmf.CreateViewStream()){//.CreateViewWrap()) {
                //Trace.Assert(vs.Position == 0L);
                var buffer = new byte[_dataSize];
                for (int i = 0; i < Length; i++) {
                    if (currentVersion != _version) throw new InvalidOperationException("Collection modified during enumeration");
                    var c = va.Read(buffer, 0, _dataSize);
                    //var buffer = va.VA.UnsafeReadBytes(i*_dataSize, _dataSize);
                    //Trace.Assert(c == _dataSize);
                    yield return Config.Serializer.Deserialize<T>(buffer);
                    Array.Clear(buffer, 0, _dataSize);
                }
            }
        }

        #endregion

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public override string ToString() {
            return string.Format("Length {0}", Length);
        }


        public void Dispose() {
            if (Fm != null) {
                Fm.Dispose();
            }
            Fm = null;
        }

        //~Array() {
        //    Trace.WriteLine("Calling finalizer on Array: " + typeof(T).Name);
        //    Dispose();
        //}

    }
}