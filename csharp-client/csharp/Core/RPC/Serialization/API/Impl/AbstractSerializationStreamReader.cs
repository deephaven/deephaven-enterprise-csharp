/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.API.Impl
{
    /// <summary>
    /// Base class for the client and server serialization streams. This class
    /// handles the basic serialization and deserialization formatting for primitive
    /// types since these are common between the client and server.
    /// </summary>
    public abstract class AbstractSerializationStreamReader : AbstractSerializationStream, ISerializationStreamReader
    {
        private readonly List<object> _seenArray = new List<object>();

        public object ReadObject()
        {
            var token = ReadInt32();

            if (token< 0)
            {
                // Negative means a previous object
                // Transform negative 1-based to 0-based.
                return _seenArray[-(token + 1)];
            }

            // Positive means a new object
            var typeSignature = GetString(token);
            if (typeSignature == null)
            {
                // a null string means a null instance
                return null;
            }

            return Deserialize(typeSignature);
        }

        /// <summary>
        /// Deserialize an object with the given type signature.
        /// </summary>
        /// <param name="typeSignature">The type signature to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        protected abstract object Deserialize(string typeSignature);

        /// <summary>
        /// Get the previously seen object at the given index which must be 1-based.
        /// </summary>
        /// <param name="index">A 1-based index into the seen objects.</param>
        /// <returns>The object stored in the seen array at index - 1.</returns>
        protected object GetDecodedObject(int index)
        {
            // index is 1-based
            return _seenArray[index - 1];
        }

        /// <summary>
        /// Gets a string out of the string table.
        /// </summary>
        /// <param name="index">The index of the string to get.</param>
        /// <returns>The string.</returns>
        protected abstract string GetString(int index);

        /// <summary>
        /// Set an object in the seen list.
        /// </summary>
        /// <param name="index">Index a 1-based index into the seen objects.</param>
        /// <param name="o">The object to remember.</param>
        protected void RememberDecodedObject(int index, object o)
        {
            // index is 1-based
            _seenArray[index - 1] = o;
        }

        /// <summary>
        /// Reserve an entry for an object in the seen list.
        /// </summary>
        /// <returns>The index to be used in future for the object.</returns>
        protected int ReserveDecodedObjectIndex()
        {
            _seenArray.Add(null);

            // index is 1-based
            return _seenArray.Count;
        }

        public abstract bool ReadBoolean();
        public abstract sbyte ReadSByte();
        public abstract char ReadChar();
        public abstract double ReadDouble();
        public abstract float ReadSingle();
        public abstract short ReadInt16();
        public abstract int ReadInt32();
        public abstract long ReadInt64();
        public abstract string ReadString();
        public abstract void ClaimItems(int slots);
    }
}
