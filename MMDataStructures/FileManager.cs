using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading;

namespace MMDataStructures {

    internal class ViewWrap : IDisposable {
        public MemoryMappedViewAccessor VA { get; }

        public ViewWrap(MemoryMappedFile mmf) {
            VA = mmf.CreateViewAccessor();
        }

        public void Dispose() {
            // fake it
        }

        ~ViewWrap() {
            VA.Dispose();
        }
    }

    internal class FileManager //: IFileManager
    {

        private readonly ThreadLocal<ViewWrap> _vw = new ThreadLocal<ViewWrap>(false);

        public ViewWrap CreateViewWrap() {
            return _vw.IsValueCreated
                ? _vw.Value
                : _vw.Value = new ViewWrap(Mmf);
        }

        private const int _GROW_PERCENTAGE = 50;
        private const int _MAX_SHARED_ACCESS = 1000;

        /// <summary>
        /// Size of Mmf in bytes
        /// </summary>
        public long Capacity { get; private set; }
        public Mutex FileMutex { get; }
        public MemoryMappedFile Mmf { get; set; }
        public PersistenceMode PersistenceMode { get; }

        private readonly string _fileName;

        private Semaphore _tempSemaphore;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="maxCapacity">Maximum capacity of MMF</param>
        /// <param name="persistenceMode"></param>
        public FileManager(string filePath, long maxCapacity, PersistenceMode persistenceMode = PersistenceMode.TemporaryPersist) {


            // sometimes we won't be able to clean the temp file. must be able to distinguish b/w persistent and temp-persistent
            if (persistenceMode == PersistenceMode.TemporaryPersist) {
                filePath = filePath + ".temp";
                var name = Path.GetFileName(filePath) + "-file-semaphore";
                _tempSemaphore = new Semaphore(_MAX_SHARED_ACCESS, _MAX_SHARED_ACCESS, name);
                _tempSemaphore.WaitOne();
            }
            FileMutex = new Mutex(Path.GetFileName(filePath) + "-file-mutex");

            _fileName = filePath;
            Capacity = maxCapacity;
            PersistenceMode = persistenceMode;

            Mmf = CreateOrOpenFile(_fileName, Capacity, PersistenceMode, FileMutex);
        }

        #region IViewManager Members

        ///// <summary>
        ///// Get a working view for the current thread
        ///// </summary>
        ///// <param name="threadId"></param>
        ///// <returns></returns>
        //public Stream GetView(int threadId)
        //{
        //    return Mmf.CreateViewStream();
        //}


        private static MemoryMappedFile CreateOrOpenFile(string fileName, long capacity, PersistenceMode persistenceMode, Mutex mutex) {
            try {
                mutex.WaitOne();
                switch (persistenceMode) {
                    case PersistenceMode.TemporaryPersist:
                    // we are first to the party since have created the mutex, will create new file instead of previous
                    // we could avoid using semaphore here because mutex.Created = semaphore.Created
                    //if (mutex.Created) { DeleteBackingFileIfExists(fileName); }
                    //goto case PersistenceMode.Persist;
                    case PersistenceMode.Persist:
                        var fileStream = new FileStream(fileName, FileMode.OpenOrCreate,
                            FileAccess.ReadWrite, FileShare.ReadWrite);
                        var mmfs = new MemoryMappedFileSecurity();

                        return MemoryMappedFile.CreateFromFile(fileStream,
                            Path.GetFileName(fileName), capacity,
                            MemoryMappedFileAccess.ReadWrite, mmfs, HandleInheritability.Inheritable,
                            false);
                    case PersistenceMode.Ephemeral:
                        return MemoryMappedFile.CreateOrOpen(fileName, capacity, MemoryMappedFileAccess.ReadWrite);
                    default:
                        throw new ArgumentOutOfRangeException("persistenceMode");
                }
            } finally {
                mutex.ReleaseMutex();
            }

        }




        /// <summary>
        /// True if MF has enough backing capacity to wrtite writeLength bytes from position
        /// </summary>
        public bool HasCapacity(long position, long writeLength) {
            return (position + writeLength) <= Capacity;
        }

        /// <summary>
        /// Grow the array to support more data
        /// </summary>
        /// <param name="requiredMinCapacity">The size to grow from</param>
        public void EnsureCapacity(long requiredMinCapacity) {
            if (Capacity >= requiredMinCapacity) return;
            FileMutex.WaitOne();
            try {
                switch (PersistenceMode) {
                    case PersistenceMode.Persist:
                    case PersistenceMode.TemporaryPersist:
                        var oldSize = Capacity;
                        var newCapacity = (long)(oldSize * ((100F + _GROW_PERCENTAGE) / 100F));
                        Capacity = newCapacity < requiredMinCapacity
                            ? requiredMinCapacity
                            : newCapacity;
                        CloseMapFile();
                        Trace.Assert(Mmf == null);
                        Mmf = CreateOrOpenFile(_fileName, Capacity, PersistenceMode, FileMutex);
                        break;
                    case PersistenceMode.Ephemeral:
                        throw new NotSupportedException("In-memory MMFs do not support resizing. Set initial capacity large enough, only actually used memory will be consumed from RAM");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            } finally {
                FileMutex.ReleaseMutex();
            }
        }



        #endregion

        #region Disposal
        //~FileManager()
        //{
        //    Dispose();
        //}

        public void Dispose() {

            _vw.Dispose();
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //try {
            //    if (_vw != null && _vw.Values != null) {
            //        var wraps = _vw.Values.ToArray();
            //        foreach (var vw in wraps) { if (vw.VA != null) { vw.VA.Dispose(); } }
            //    }
            //} catch  { }

            try {
                FileMutex.WaitOne();
                CloseMapFile();
                if (PersistenceMode == PersistenceMode.TemporaryPersist) {
                    var count = _tempSemaphore.Release();
                    // if no more access to the file, then delete it
                    if (count == _MAX_SHARED_ACCESS - 1) {
                        DeleteBackingFileIfExists();
                    }
                }
            } finally {
                FileMutex.ReleaseMutex();
            }
            _tempSemaphore.Dispose();
            FileMutex.Dispose();
        }

        private void CloseMapFile() {
            if (Mmf == null) {
                Trace.TraceWarning("Mmf is null in CloseMapFile");
                return;
            }
            Mmf.Dispose();
            Mmf = null;
        }

        private void DeleteBackingFileIfExists() {

            //try
            //{
            if (!File.Exists(_fileName)) return;
            Trace.WriteLine("Deleting file: " + _fileName);
            Thread.Sleep(2000);
            File.Delete(_fileName);
            //}
            //catch (UnauthorizedAccessException e)
            //{
            // TODO: Handle files which for some reason didn't want to be deleted
            //   Trace.WriteLine(e.Message);
            //}
        }

        #endregion


    }
}