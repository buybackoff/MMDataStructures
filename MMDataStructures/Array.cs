using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;


namespace MMDataStructures
{
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
        where T : struct
    {
        #region Private/Protected
        private int _dataSize;
        protected ReaderWriterLockSlim ValueLock = new ReaderWriterLockSlim();
        protected IViewManager ViewManager;
        private int _version;
        #endregion

        #region Properties

        public bool KeepFile
        {
            get
            {
                return ViewManager.KeepFile;
            }
            set
            {
                ViewManager.KeepFile = value;
            }
        }

        /// <summary>
        /// Allow array to automatically grow if you access an indexer larger than the starting size
        /// </summary>
        public bool AutoGrow { get; set; }

        /// <summary>
        /// Return the number of elements in the array
        /// </summary>
        public virtual long Length
        {
            get { return ViewManager.Length; }
        }

        private long Capacity
        {
            get { return Length; }
        }

        /// <summary>
        /// Set the position before setting or getting data
        /// </summary>
        public long Position
        {
            get
            {
                int threadId = Thread.CurrentThread.ManagedThreadId;
                Stream s = ViewManager.GetView(threadId);
                return s.Position;
            }
            set
            {
                int threadId = Thread.CurrentThread.ManagedThreadId;
                Stream s = ViewManager.GetView(threadId);
                s.Position = value * _dataSize;
            }
        }

        public override string ToString()
        {
            return string.Format("Length {0}", Length);
        }

        public void Dispose()
        {
            if (ViewManager != null)
            {
                ViewManager.Dispose();
            }
            ViewManager = null;
        }

        #endregion

        /// <summary>
        /// Create a new memory mapped array on disk
        /// </summary>
        /// <param name="capacity">The length of the array to allocate</param>
        /// <param name="path">The directory where the memory mapped file is to be stored</param>
        public Array(long capacity, string path)
            : this(capacity, path, false)
        {
        }

        /// <summary>
        /// Create a new memory mapped array on disk
        /// </summary>
        /// <param name="capacity">The number of elements to allocate in the array</param>
        /// <param name="path">The directory where the memory mapped file is to be stored</param>
        /// <param name="autoGrow">Decide if the array can expand or not</param>
        public Array(long capacity, string path, bool autoGrow)
            : this(capacity, path, autoGrow, new ViewManager())
        {
        }

        public Array(long capacity, string path, bool autoGrow, IViewManager viewManager)
        {
            ViewManager = viewManager;

            string fileName;
            if(File.Exists(path) || Path.GetExtension(path).Length > 0)
            {
                fileName = path;
            }
            else
            {
                fileName = Path.Combine(path, "mmf-" + Guid.NewGuid() + ".bin");
            }            
            AutoGrow = autoGrow;

            InitWorkerBuffers();

            ViewManager.Initialize(fileName, capacity, _dataSize);
        }

        ~Array()
        {
            Trace.WriteLine("Calling finalizer on Array:" + typeof(T).Name);
            Dispose();
        }

        private void InitWorkerBuffers()
        {
            _dataSize = Marshal.SizeOf(typeof(T));
        }


        internal void Write(byte[] buffer, long index)
        {
            Stream viewStream = GetThreadStream();
            viewStream.Position = index * _dataSize;
            WriteBufferToStream(buffer);
        }

        public void Write(byte[] buffer)
        {
            WriteBufferToStream(buffer);
        }

        internal void Write(byte[] buffer, int bufferLength)
        {
            WriteBufferToStream(buffer, bufferLength);
        }

        internal void WriteByte(byte b)
        {
            Stream viewStream = GetThreadStream();
            WriteBufferToStream(viewStream, b);
        }

        private Stream GetThreadStream()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            return ViewManager.GetView(threadId);
        }

        private void WriteBufferToStream(byte[] buffer)
        {
            WriteBufferToStream(buffer, buffer.Length);
        }

        private void WriteBufferToStream(byte[] buffer, int bufferLength)
        {
            Stream viewStream = GetThreadStream();
            if (NeedToGrowView(viewStream.Position, buffer.LongLength))
            {
                viewStream = GrowViewAndGetNewStream(viewStream.Position, buffer.LongLength);
            }
            viewStream.Write(buffer, 0, bufferLength);
            _version++;
        }

        private void WriteBufferToStream(Stream viewStream, byte b)
        {
            if (NeedToGrowView(viewStream.Position, 1))
            {
                viewStream = GrowViewAndGetNewStream(viewStream.Position, 1);
            }
            viewStream.WriteByte(b);
            _version++;
        }

        private bool NeedToGrowView(long streamPosition, long length)
        {
            return AutoGrow && !ViewManager.EnoughBackingCapacity(streamPosition, length);
        }

        private Stream GrowViewAndGetNewStream(long originalPosition, long length)
        {
            ViewManager.Grow(originalPosition + length);
            Stream viewStream = GetThreadStream();
            viewStream.Position = originalPosition;
            return viewStream;
        }

        /// <summary>
        /// Reads a T from the current position
        /// </summary>
        /// <returns>Byte array of the size of T</returns>
        public byte[] Read()
        {
            var bytes = MultiRead(_dataSize);
            return bytes;
        }

        /// <summary>
        /// Reads a T from the index position
        /// </summary>
        /// <returns>Byte array of the size of T</returns>
        public byte[] Read(long index)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Stream s = ViewManager.GetView(threadId);
            s.Position = index * _dataSize;

            byte[] buffer = new byte[_dataSize];
            return FillBufferFromStream(buffer, s);
        }

        public byte[] MultiRead(int count)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            Stream s = ViewManager.GetView(threadId);
            byte[] buffer = new byte[count];
            return FillBufferFromStream(buffer, s);
        }

        private static byte[] FillBufferFromStream(byte[] buffer, Stream stream)
        {
            int totalBytesToRead = buffer.Length;
            int totalBytesRead = 0;
            while (totalBytesToRead > 0)
            {
                int bytesRead = stream.Read(buffer, totalBytesRead, totalBytesToRead);
                if (bytesRead == 0)
                    break;
                totalBytesRead += bytesRead;
                totalBytesToRead -= bytesRead;
            }
            return buffer;
        }

        internal byte ReadByte()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Stream s = ViewManager.GetView(threadId);
            return (byte)s.ReadByte();
        }

        public T this[long index]
        {
            get
            {
                if (index < 0)
                {
                    throw new IndexOutOfRangeException("Tried to access item outside the array boundaries");
                }
                ValueLock.EnterReadLock();
                if (index >= Capacity)
                {
                    if (AutoGrow)
                    {
                        ViewManager.Grow(index);
                    }
                    else
                    {
                        throw new IndexOutOfRangeException("Tried to access item outside the array boundaries");
                    }
                }
                try
                {
                    return Config.Serializer.Deserialize<T>(Read(index));
                }
                finally
                {
                    ValueLock.ExitReadLock();
                }
            }
            set
            {
                if (index < 0)
                {
                    throw new IndexOutOfRangeException("Tried to access item outside the array boundaries");
                }
                ValueLock.EnterWriteLock();
                try
                {
                    if (index >= Capacity)
                    {
                        if (AutoGrow)
                        {
                            ViewManager.Grow(index);
                        }
                        else
                        {
                            throw new IndexOutOfRangeException("Tried to access item outside the array boundaries");
                        }
                    }
                    Write(Config.Serializer.Serialize(value), index);
                }
                finally
                {
                    ValueLock.ExitWriteLock();
                }
            }
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            int currentVersion = _version;
            Position = 0;
            long iterLength = Length;
            for (int i = 0; i < iterLength; i++)
            {
                if (currentVersion != _version) throw new InvalidOperationException("Collection modified during enumeration");
                yield return Config.Serializer.Deserialize<T>(Read());
            }
        }

        #endregion

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}