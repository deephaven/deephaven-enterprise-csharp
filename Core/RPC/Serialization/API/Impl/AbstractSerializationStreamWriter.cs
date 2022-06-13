/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.API.Impl
{
    /// <summary>
    /// Base class for the client and server serialization streams. This class handles
    /// basic serialization and deserialization formatting for primitive types since
    /// these are common between the client and server. It also handles Object- and
    /// String-tracking for building graph references.
    /// </summary>
    public abstract class AbstractSerializationStreamWriter : AbstractSerializationStream, ISerializationStreamWriter
    {
        private int _objectCount;
        private readonly Dictionary<object, int> _objectMap = new Dictionary<object, int>();
        private readonly Dictionary<string, int> _stringMap = new Dictionary<string, int>();
        private readonly List<string> _stringTable = new List<string>();

        public void PrepareToWrite()
        {
            _objectCount = 0;
            _objectMap.Clear();
            _stringMap.Clear();
            _stringTable.Clear();
        }

        public virtual void Write(bool fieldValue)
        {
            Append(fieldValue ? "1" : "0");
        }

        public virtual void Write(sbyte fieldValue)
        {
            Append(Convert.ToChar(fieldValue).ToString());
        }

        public virtual void Write(char fieldValue)
        {
            // just use an int, it's more foolproof
            Append(Convert.ToInt32(fieldValue).ToString());
        }

        public virtual void Write(double fieldValue)
        {
            Append(fieldValue.ToString());
        }

        public virtual void Write(float fieldValue)
        {
            Write((double) fieldValue);
        }

        public virtual void Write(int fieldValue)
        {
            Append(fieldValue.ToString());
        }

        public abstract void Write(long value);

        public virtual void WriteObject(object instance)
        {
            if (instance == null)
            {
                // write a null string
                Write((string)null);
                return;
            }

            var objIndex = GetIndexForObject(instance);
            if (objIndex >= 0)
            {
                // We've already encoded this object, make a backref
                // Transform 0-based to negative 1-based
                Write(-(objIndex + 1));
                return;
            }

            SaveIndexForObject(instance);

            // Serialize the type signature
            var typeSignature = GetObjectTypeSignature(instance);
            if (typeSignature == null)
            {
                throw new SerializationException(
                        "could not get type signature for " + instance.GetType());
            }
            Write(typeSignature);
            // Now serialize the rest of the object
            Serialize(instance, typeSignature);
        }

        public virtual void Write(short value)
        {
            Append(value.ToString());
        }

        public virtual void Write(string value)
        {
            Write(AddString(value));
        }

        /// <summary>
        /// Add a string to the string table and return its index.
        /// </summary>
        /// <param name="str">The string to add.</param>
        /// <returns>The index to the string.</returns>
        protected int AddString(string str)
        {
            if (str == null)
            {
                return 0;
            }
            if(_stringMap.ContainsKey(str))
            {
                return _stringMap[str];
            }
            _stringTable.Add(str);
            // index is 1-based
            var index = _stringTable.Count;
            _stringMap.Add(str, index);
            return index;
        }

        /// <summary>
        /// Append a token to the underlying output buffer.
        /// </summary>
        /// <param name="token">The token to append.</param>
        protected abstract void Append(string token);

        /// <summary>
        /// Get the index for an object that may have previously been saved via <see cref="SaveIndexForObject(object)"/>
        /// </summary>
        /// <param name="instance">The object to save.</param>
        /// <returns>The index associated with this object, or -1 if this object hasn't been seen before.</returns>
        protected int GetIndexForObject(object instance)
        {
            return _objectMap.ContainsKey(instance) ? _objectMap[instance] : -1;
        }

        /// <summary>
        /// Compute and return the type signature for an object.
        /// </summary>
        /// <param name="instance">The instance to inspect.</param>
        /// <returns>The type signature of the instance.</returns>
        protected abstract string GetObjectTypeSignature(object instance);

        /// <summary>
        /// Gets the string table.
        /// </summary>
        /// <returns>The string table.</returns>
        protected List<string> GetStringTable()
        {
            return _stringTable;
        }

        /// <summary>
        /// Remember this object as having been seen before.
        /// </summary>
        /// <param name="instance">Instance the object to remember.</param>
        protected void SaveIndexForObject(object instance)
        {
            _objectMap.Add(instance, _objectCount++);
        }

        /// <summary>
        /// Serialize an object into the stream.
        /// </summary>
        /// <param name="instance">The object to serialize.</param>
        /// <param name="typeSignature">The type signature of the object.</param>
        protected abstract void Serialize(object instance, string typeSignature);
    }
}
