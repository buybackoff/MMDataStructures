using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MMDataStructures
{
    class ByteArray : Array<byte>
    {
        public ByteArray(long capacity, string path)
            : base(capacity, path)
        {
        }

        public ByteArray(long capacity, string path, bool autoGrow)
            : base(capacity, path, autoGrow)
        {
        }

        public ByteArray(long capacity, string path, bool autoGrow, IViewManager viewManager)
            : base(capacity, path, autoGrow, viewManager)
        {
        }

        /// <summary>Writes an long in a variable-length format.  Writes between one and five
        /// bytes.  Smaller values take fewer bytes.  Negative numbers are not
        /// supported.
        /// </summary>
        public byte WriteVLong(long value)
        {
            byte[] buffer = new byte[9];
            byte length = 0;
            while ((value & ~0x7F) != 0)
            {
                buffer[length] = (byte)((value & 0x7f) | 0x80);
                value = value >> 7;
                length++;
            }
            if (length == 0)
            {
                WriteByte((byte)value);
                length++;
            }
            else
            {
                buffer[length] = (byte)value;
                length++;
                Write(buffer, length);
            }
            return length;
        }

        public byte WriteVInt(int value)
        {
            byte[] buffer = new byte[5];
            byte length = 0;
            while ((value & ~0x7F) != 0)
            {
                buffer[length] = (byte)((value & 0x7f) | 0x80);
                value = value >> 7;
                length++;
            }
            if (length == 0)
            {
                WriteByte((byte)value);
                length++;
            }
            else
            {
                buffer[length] = (byte)value;
                length++;
                Write(buffer, length);
            }
            return length;
        }

        /// <summary>Reads a long stored in variable-length format.  Reads between one and
        /// nine bytes.  Smaller values take fewer bytes.  Negative numbers are not
        /// supported. 
        /// </summary>
        public long ReadVLong()
        {
            byte b = ReadByte();
            long i = b & 0x7F;
            for (int shift = 7; (b & 0x80) != 0; shift += 7)
            {
                b = ReadByte();
                i |= (b & 0x7FL) << shift;
            }
            return i;
        }

        public int ReadVInt()
        {
            byte b = ReadByte();
            int i = b & 0x7F;
            for (int shift = 7; (b & 0x80) != 0; shift += 7)
            {
                b = ReadByte();
                i |= (b & 0x7F) << shift;
            }
            return i;
        }
    }
}
