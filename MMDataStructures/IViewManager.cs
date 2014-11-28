using System.IO;

namespace MMDataStructures
{
    public interface IViewManager
    {
        /// <summary>
        /// Number of items in the file
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Get a working view for the current thread
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        Stream GetView(int threadId);

        /// <summary>
        /// Initialize the backing file
        /// </summary>
        /// <param name="fileName">Filename to store the data</param>
        /// <param name="capacity">Number of items to allocate</param>
        /// <param name="dataSize">Size of datastructure</param>
        void Initialize(string fileName, long capacity, int dataSize);

        /// <summary>
        /// Verify that the persisting file is large enough for the data written
        /// </summary>
        /// <param name="position">Position to start writing</param>
        /// <param name="writeLength">Number of bytes to write</param>
        /// <returns></returns>
        bool EnoughBackingCapacity(long position, long writeLength);

        /// <summary>
        /// Grow file
        /// </summary>
        /// <param name="sizeToGrowFrom">Size to grow from. Could be max size or an offset larger than the file</param>
        void Grow(long sizeToGrowFrom);

        /// <summary>
        /// Remove the backing file
        /// </summary>
        void Dispose();

        /// <summary>
        /// Keep file on exit
        /// </summary>
        bool KeepFile { get; set; }
    }
}