using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
//using Newtonsoft.Json;

// From Ractor.Persistence
// Commented out code is left intentionally as an example of how to plug in any other serializer

namespace MMDataStructures {
    public interface ISerializer {
        byte[] Serialize<T>(T value);
        T Deserialize<T>(byte[] bytes);
        T DeepClone<T>(T value);
    }

    // TODO how to correctly deal with null? throw here or pass downstream?
    /// <summary>
    /// 
    /// </summary>
    public class DefaultSerializer : ISerializer {
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        /// <summary>
        /// 
        /// </summary>
        public byte[] Serialize<T>(T value) {
            if (typeof(T).IsValueType) {
                return StructToBytes(value);
            } else {
                MemoryStream byteStream = new MemoryStream();
                _formatter.Serialize(byteStream, value);
                byteStream.Position = 0;
                return byteStream.ToArray();

                //if (!typeof(T).IsValueType && EqualityComparer<T>.Default.Equals(value, default(T)))
                //{
                //    return null;
                //}
                //var json = JsonConvert.SerializeObject(value);
                //return Encoding.UTF8.GetBytes(json);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public T Deserialize<T>(byte[] bytes) {

            if (typeof(T).IsValueType) {
                return BytesToStruct<T>(bytes);
            } else {
                MemoryStream byteStream = new MemoryStream(bytes);
                return (T)_formatter.UnsafeDeserialize(byteStream, null);

                //if (bytes == null) return default(T);
                //var json = Encoding.UTF8.GetString(bytes);
                //return JsonConvert.DeserializeObject<T>(json);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public T DeepClone<T>(T value) {
            return Deserialize<T>(Serialize(value));
        }


        private byte[] StructToBytes<TS>(TS str) {
            if (str is Int64) { return BitConverter.GetBytes(Convert.ToInt64(str)); }

            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        private TS BytesToStruct<TS>(byte[] arr) {
            if (typeof(TS) == typeof(Int64)) {
                return (TS)((object)BitConverter.ToInt64(arr, 0));
            }
            TS str = default(TS);
            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (TS)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            return str;
        }

    }


    /// <summary>
    /// 
    /// </summary>
    public static class CommonExtentions {


        /// <summary>
        /// In-memory compress
        /// </summary>
        public static byte[] GZip(this byte[] bytes) {
            using (var inStream = new MemoryStream(bytes)) {
                using (var outStream = new MemoryStream()) {
                    using (var compress = new GZipStream(outStream, CompressionMode.Compress)) {
                        inStream.CopyTo(compress);
                    }
                    return outStream.ToArray();
                }
            }
        }


        /// <summary>
        /// In-memory uncompress
        /// </summary>
        public static byte[] UnGZip(this byte[] bytes) {
            byte[] outBytes;
            using (var inStream = new MemoryStream(bytes)) {
                using (var outStream = new MemoryStream()) {
                    using (var deCompress = new GZipStream(inStream, CompressionMode.Decompress)) {
                        deCompress.CopyTo(outStream);
                    }
                    outBytes = outStream.ToArray();
                }
            }
            return outBytes;
        }

    }

}