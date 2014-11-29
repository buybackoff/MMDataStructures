using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MMDataStructures.DictionaryBacking {
    /// <summary>
    /// Persist a Dictionary on disk. One file for hashes, one for keys and one for values.
    /// Keys and values can be of variable sizes.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class BackingUnknownSize<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly byte[] EmptyPosition = new byte[8];

        private readonly int _capacity;

        private Array<long> _hashCodeLookup;
        private Array<byte> _keys;
        private Array<byte> _values;
        private long _largestSeenKeyPosition = 1; // set start position to 1 to simplify logic
        private long _largestSeenValuePosition;
        private string _path;
        private int _defaultKeySize;
        private int _defaultValueSize;
        private string _controlFile;
        private string _hashFile;
        private string _keyFile;
        private string _valueFile;

        private int _version;

        internal Mutex Mutex { get { return _keys.Fm.FileMutex; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of the dictionary and its backing file</param>
        /// <param name="capacity">Number of buckets for the hash</param>
        /// <param name="persistenceMode"></param>
        public BackingUnknownSize(string name, int capacity = 1000, PersistenceMode persistenceMode = PersistenceMode.TemporaryPersist) {
            _capacity = HashHelpers.GetNextPrime(capacity);
            SetStorageFilenames(name);
            if (persistenceMode == PersistenceMode.Persist) {
                InitializeControlFile();
            }

            SetDefaultKeyValueSize();

            _hashCodeLookup = new Array<long>(_capacity, _hashFile, true, persistenceMode);
            _keys = new Array<byte>(_capacity * _defaultKeySize, _keyFile, true, persistenceMode);
            _values = new Array<byte>(_capacity * _defaultValueSize, _valueFile, true, persistenceMode);

            InitDictionary();
        }

        private void SetStorageFilenames(string name) {
            if (string.IsNullOrEmpty(name)) {
                name = Guid.NewGuid().ToString();
            }
            _path = Config.DataPath;
            if (!Directory.Exists(_path)) { Directory.CreateDirectory(_path); }
            _controlFile = Path.Combine(_path, name + ".ctrl");
            _hashFile = Path.Combine(_path, name + ".hash");
            _keyFile = Path.Combine(_path, name + ".key");
            _valueFile = Path.Combine(_path, name + ".value");
        }

        private void InitializeControlFile() {
            // TODO weird, must be able to create dic without setting capacity, but read existing

            if (File.Exists(_controlFile)) {
                if (!File.Exists(_hashFile)) throw new FileNotFoundException("Hash file is missing");
                if (!File.Exists(_keyFile)) throw new FileNotFoundException("Key file is missing");
                if (!File.Exists(_valueFile)) throw new FileNotFoundException("Value file is missing");

                var settings = File.ReadAllLines(_controlFile);
                var ctrlCapacity = int.Parse(settings[0]);
                if (ctrlCapacity != _capacity) throw new ArgumentException("capacity differes from existing file. " + _capacity + " != " + ctrlCapacity);

            } else {
                if (File.Exists(_hashFile)) throw new InvalidOperationException("Hash file exists already");
                if (File.Exists(_keyFile)) throw new InvalidOperationException("Key file exists already");
                if (File.Exists(_valueFile)) throw new InvalidOperationException("Value file exists already");

                using (var fileStream = File.CreateText(_controlFile)) {
                    fileStream.WriteLine(_capacity.ToString());
                }
            }
        }

        private void InitDictionary() // TODO why this is needed?
        {
            Trace.WriteLine("Initializing dictionary - reading existing values");
            foreach (var kvp in this) {
                //TODO: read in largest seen pos values
                Count++;
                if (Count % 100000 == 0) {
                    Trace.WriteLine("Items read: " + Count);
                }
            }
        }

        void SetDefaultKeyValueSize() {
            if (default(TKey) != null)
                _defaultKeySize = Config.Serializer.Serialize(default(TKey)).Length;
            if (default(TValue) != null)
                _defaultValueSize = Config.Serializer.Serialize(default(TValue)).Length;

            if (_defaultKeySize == 0) _defaultKeySize = 40;
            if (_defaultValueSize == 0) _defaultValueSize = 40;
        }

        //~BackingUnknownSize() {
        //    Dispose();
        //}

        public void Dispose() {
            if (_keys != null) _keys.Dispose();
            if (_values != null) _values.Dispose();
            if (_hashCodeLookup != null) {
                if (_hashCodeLookup.PersistenceMode != PersistenceMode.Persist) {
                    File.Delete(_controlFile);
                }
                _hashCodeLookup.Dispose();
            }
            _keys = null;
            _values = null;
            _hashCodeLookup = null;
        }

        private int GetHashCodePosition(TKey key) {
            int num = key.GetHashCode() & 0x7fffffff;
            int index = num % _capacity;
            return index;
        }

        #region Implementation of IEnumerable

        /*
        File Layout Keyfile
        
        KeyLength   VInt  -> Int
        KeyBytes[]
        ValuePos    VLong  -> Long
        NextKeyPos  VLong -  -> Long
        
        File Layout Valuefile
        ValueLength   VInt
        ValueBytes[]
         */

        /// <summary>
        /// Enumerate over the dictionary
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            using (var kva = _keys.Fm.Mmf.CreateViewAccessor())
            using (var vva = _values.Fm.Mmf.CreateViewAccessor()) {
                // iterating using view stream in _hashCodeLookup array
                foreach (var firstKeyPosition in _hashCodeLookup) {
                    var keyPosition = firstKeyPosition;
                    while (keyPosition != 0) {
                        var keyCursor = keyPosition;
                        var keyLength = kva.ReadInt32(keyCursor);
                        keyCursor = keyCursor + 4;

                        var keyBytes = new byte[keyLength];
                        kva.ReadArray(keyCursor, keyBytes, 0, keyLength);
                        keyCursor = keyCursor + keyLength;

                        var key = Config.Serializer.Deserialize<TKey>(keyBytes);

                        var valuePos = kva.ReadInt64(keyCursor);
                        keyCursor = keyCursor + 8;

                        var valueCursor = valuePos;
                        var valueLength = vva.ReadInt32(valueCursor);
                        valueCursor = valueCursor + 4;

                        var valueBytes = new byte[valueLength];
                        vva.ReadArray(valueCursor, valueBytes, 0, valueLength);
                        valueCursor = valueCursor + valueLength;

                        var value = Config.Serializer.Deserialize<TValue>(valueBytes);

                        if (keyCursor > _largestSeenKeyPosition)
                            _largestSeenKeyPosition = keyCursor + 8;
                        if (valueCursor > _largestSeenValuePosition)
                            _largestSeenValuePosition = valueCursor;

                        yield return new KeyValuePair<TKey, TValue>(key, value);
                        keyPosition = kva.ReadInt64(keyCursor);
                    }
                }
            }
        }

        /// <summary>
        /// Enumerate over the dictionary
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IDictionaryPersist<TKey,TValue>

        public int Count { get; set; }

        public bool ContainsKey(TKey key) {
            TValue value;
            return TryGetValue(key, out value);
        }

        public bool ContainsValue(TValue value) {
            byte[] valueBytes = Config.Serializer.Serialize(value);

            IEnumerator<KeyValuePair<TKey, TValue>> enumerator = GetEnumerator();
            while (enumerator.MoveNext()) {
                if (ByteCompare(valueBytes, enumerator.Current.Value)) return true;
            }
            return false;
        }

        /*
        File Layout Keyfile
        
        KeyLength   VInt  -> Int
        KeyBytes[]
        ValuePos    VLong  -> Long
        NextKeyPos  VLong -  -> Long
        
        File Layout Valuefile
        ValueLength   VInt
        ValueBytes[]
         */

        public void Add(TKey key, TValue value) {
            var pos = GetHashCodePosition(key);
            var keyPosition = _hashCodeLookup[pos];
            var keyBytesNew = Config.Serializer.Serialize(key);
            using (var kva = _keys.Fm.Mmf.CreateViewAccessor()) {
                if (keyPosition != 0) {
                    long nextKeyCursor;
                    do {
                        var keyCursor = keyPosition;
                        var keyLengthExisting = kva.ReadInt32(keyCursor);

                        keyCursor = keyCursor + 4;

                        var keyBytesExisting = new byte[keyLengthExisting];
                        kva.ReadArray(keyCursor, keyBytesExisting, 0, keyLengthExisting);
                        keyCursor = keyCursor + keyLengthExisting;

                        if (ByteArrayCompare.Equals(keyBytesNew, keyBytesExisting)) {
                            throw new ArgumentException("An item with the same key has already been added.");
                        }
                        keyCursor = keyCursor + 8; // skip valuepos
                        nextKeyCursor = keyCursor;
                        keyPosition = kva.ReadInt64(keyCursor); // next key pos with same hash
                    } while (keyPosition != 0);
                    _keys.Fm.EnsureCapacity(nextKeyCursor + 8);
                    kva.Write(nextKeyCursor, _largestSeenKeyPosition); // Fill in the chained keyhash

                } else {
                    _hashCodeLookup[pos] = _largestSeenKeyPosition;
                }

                var keyLengthNew = keyBytesNew.Length;
                var cursor = _largestSeenKeyPosition;
                _keys.Fm.EnsureCapacity(cursor + 4);
                kva.Write(cursor, keyLengthNew);
                cursor = cursor + 4;
                _keys.Fm.EnsureCapacity(cursor + keyLengthNew);
                kva.WriteArray(cursor, keyBytesNew, 0, keyLengthNew);
                cursor = cursor + keyLengthNew;
                _keys.Fm.EnsureCapacity(cursor + 8);
                kva.Write(cursor, _largestSeenValuePosition);
                cursor = cursor + 8;
                _keys.Fm.EnsureCapacity(cursor + 8);
                kva.WriteArray(cursor, EmptyPosition, 0, 8); // space for nextkeypos
                cursor = cursor + 8;

                _largestSeenKeyPosition = cursor;
                _version++;
            }

            var valueBytes = Config.Serializer.Serialize(value);
            using (var vva = _values.Fm.Mmf.CreateViewAccessor()) {
                var cursor = _largestSeenValuePosition;
                var valuesLength = valueBytes.Length;
                _values.Fm.EnsureCapacity(cursor + 4);
                vva.Write(cursor, valuesLength);
                cursor = cursor + 4;

                _values.Fm.EnsureCapacity(cursor + valuesLength);
                vva.WriteArray(cursor, valueBytes, 0, valuesLength);
                cursor = cursor + valuesLength;
                _largestSeenValuePosition = cursor;
            }
            Count++;
        }

        public bool Remove(TKey key) {
            var pos = GetHashCodePosition(key);
            var nextKeyFilePos = _hashCodeLookup[pos];

            var keyBytes = Config.Serializer.Serialize(key);
            var updateKeyPos = nextKeyFilePos;
            if (nextKeyFilePos == 0) return false;

            using (var kva = _keys.Fm.Mmf.CreateViewAccessor()) {
                var keysWithSameHash = 0;
                do {
                    keysWithSameHash++;
                    var keyCursor = nextKeyFilePos;

                    var keyLengthExisting = kva.ReadInt32(keyCursor);
                    keyCursor = keyCursor + 4;

                    var keyBytesExisting = new byte[keyLengthExisting];
                    kva.ReadArray(keyCursor, keyBytesExisting, 0, keyLengthExisting);
                    keyCursor = keyCursor + keyLengthExisting;

                    keyCursor = keyCursor + 8; // skip valuepos
                    var newUpdateKeyPos = keyCursor; // store pos space for chained key pos
                    nextKeyFilePos = kva.ReadInt64(keyCursor); // next key pos with same hash

                    if (ByteArrayCompare.Equals(keyBytes, keyBytesExisting)) {
                        if (keysWithSameHash == 1) {
                            _hashCodeLookup[pos] = nextKeyFilePos;
                        } else {
                            _keys.Fm.EnsureCapacity(updateKeyPos + 8);
                            kva.Write(updateKeyPos, nextKeyFilePos);
                        }
                        Count--;
                        _version++;
                        return true;
                    }
                    updateKeyPos = newUpdateKeyPos;
                } while (nextKeyFilePos != 0);
            }
            return false;
        }

        /*
       File Layout Keyfile

       KeyLength   VInt  -> Int
       KeyBytes[]
       ValuePos    VLong  -> Long
       NextKeyPos  VLong -  -> Long

       File Layout Valuefile
       ValueLength   VInt
       ValueBytes[]
        */

        public bool TryGetValue(TKey key, out TValue value) {
            var pos = GetHashCodePosition(key);
            var keyFilePos = _hashCodeLookup[pos];

            byte[] keyBytes = Config.Serializer.Serialize(key);
            using (var kva = _keys.Fm.Mmf.CreateViewAccessor())
            using (var vva = _values.Fm.Mmf.CreateViewAccessor()) {
                while (keyFilePos != 0) {
                    var keyCursor = keyFilePos;
                    int keyLengthExisting = kva.ReadInt32(keyCursor);
                    keyCursor = keyCursor + 4;

                    var keyBytesExisting = new byte[keyLengthExisting];
                    kva.ReadArray(keyCursor, keyBytesExisting, 0, keyLengthExisting);
                    keyCursor = keyCursor + keyLengthExisting;

                    if (ByteArrayCompare.Equals(keyBytes, keyBytesExisting)) {
                        // we have match on the key
                        var valCursor = kva.ReadInt64(keyCursor);
                        // increment keyCursor after the if block
                        int valLength = vva.ReadInt32(valCursor);
                        valCursor += 4;
                        var valBytes = new byte[valLength];
                        vva.ReadArray(valCursor, valBytes, 0, valLength);
                        value = Config.Serializer.Deserialize<TValue>(valBytes);
                        return true;
                    }
                    keyCursor += 8; // skip valueposition
                    keyFilePos = kva.ReadInt64(keyCursor); // _keys.ReadVLong(); // next key pos with same hash
                }
            }

            value = default(TValue);
            return false;
        }

        public bool ByteCompare(TValue value, TValue existing) {
            return ByteArrayCompare.UnSafeEquals(Config.Serializer.Serialize(value),
                                                 Config.Serializer.Serialize(existing));

        }

        public bool ByteCompare(byte[] value, TValue existing) {
            return ByteArrayCompare.UnSafeEquals(value, Config.Serializer.Serialize(existing));
        }

        public IEnumerable<TKey> AllKeys() {
            using (var kva = _keys.Fm.Mmf.CreateViewAccessor()) {
                foreach (long firstKeyPosition in _hashCodeLookup) {
                    long keyPosition = firstKeyPosition;
                    while (keyPosition != 0) {
                        var keyCursor = keyPosition;
                        int keyLength = kva.ReadInt32(keyCursor); // _keys.ReadVInt();
                        keyCursor += 4;
                        var keyBytesExisting = new byte[keyLength];
                        kva.ReadArray(keyCursor, keyBytesExisting, 0, keyLength);
                        keyCursor = keyCursor + keyLength;

                        var key = Config.Serializer.Deserialize<TKey>(keyBytesExisting);
                        keyCursor += 8; // skip value pos
                        yield return key;
                        keyPosition = kva.ReadInt64(keyCursor);
                    }
                }
            }
        }

        public IEnumerable<TValue> AllValues() {
            IEnumerator<KeyValuePair<TKey, TValue>> enumerator = GetEnumerator();
            while (enumerator.MoveNext()) {
                yield return enumerator.Current.Value;
            }
        }

        public void Clear() {
            // TODO wrong logic here for persistent vs temp vs in-memory? 
            _hashCodeLookup = new Array<long>(_capacity, _path, true);
            _keys = new Array<byte>(_capacity * _defaultKeySize, _path, true);
            _values = new Array<byte>(_capacity * _defaultValueSize, _path, true);
            _largestSeenKeyPosition = 1;
            _largestSeenValuePosition = 0;
            Count = 0;
            _version++;
        }
        #endregion
    }
}
