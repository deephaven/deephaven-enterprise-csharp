/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System.IO;
using System.Text;
using Deephaven.OpenAPI.Core.RPC.Serialization.API;
using Deephaven.OpenAPI.Core.RPC.Serialization.API.Impl;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Stream.Binary
{
    /// <summary>
    /// Simple binary serialization stream writer, which encodes the payload into a MemoryStream, but stores the strings
    /// in a string table.
    /// </summary>
    public class BinarySerializationStreamWriter : AbstractSerializationStreamWriter
    {
        public const int InitBufSize = 1024;

        private MemoryStream _memoryStream;
        private BinaryWriter _binaryWriter;
        private readonly ITypeSerializer _serializer;

        /// <summary>
        /// Create a new binary serialization stream writer, using the given type serializer for writing objects.
        /// </summary>
        /// <param name="serializer">The type serializer that will provide FieldSerializer instances for writing objects.</param>
        public BinarySerializationStreamWriter(ITypeSerializer serializer)
        {
            _serializer = serializer;
            _memoryStream = new MemoryStream(InitBufSize);
            _binaryWriter = new BinaryWriter(_memoryStream);
            _memoryStream.Position = 3 << 2;
        }

        /// <summary>
        /// Gets the bytes for the stream. Can only be called once, will prevent more data from being written.
        /// </summary>
        /// <returns>A memory stream containing all data written, with with appropriate length and position at zero.</returns>
        public MemoryStream GetPayloadBytes()
        {
            _memoryStream.SetLength(_memoryStream.Position); // truncate buffer to size of written data
            _memoryStream.Position = 0;    // prepare to write version, flags & length
            _binaryWriter.Write(GetVersion());
            _binaryWriter.Write(GetFlags());
            _binaryWriter.Write((int)(_memoryStream.Length - (3 << 2)));
            _memoryStream.Position = 0; // reset position to start
            var retVal = _memoryStream;
            _memoryStream = null;
            _binaryWriter = null;
            return retVal;
        }

        public string[] GetFinishedStringTable()
        {
            var stringTable = GetStringTable();
            return stringTable.ToArray();
        }

        public MemoryStream GetFullPayload()
        {
            var payloadBytes = GetPayloadBytes();
            var stringTable = GetStringTable();
            var stringCount = stringTable.Count;
            if (stringCount == 0)
            {
                return payloadBytes;
            }

            // make a buffer big enough to store all strings (and lengths)

            // start with the size of the regular data, one int per string, and one int to count the strings
            var size = (int)(payloadBytes.Position + ((1 + stringCount) << 2));

            // add the length of each string in bytes, and store the converted strings so we don't need to do it again
            var stringBytes = new byte[stringCount][];
            for (var i = 0; i < stringCount; i++)
            {
                var bytes = Encoding.UTF8.GetBytes(stringTable[i]);
                stringBytes[i] = bytes;
                size += bytes.Length;
            }
            var bb = new MemoryStream(size);
            var bbWriter = new BinaryWriter(bb);

            // append the payload, the count of strings, and the strings themselves
            bbWriter.Write(payloadBytes.GetBuffer(), 0, (int)payloadBytes.Length);
            bbWriter.Write(stringCount);
            for (var i = 0; i < stringCount; i++)
            {
                var bytes = stringBytes[i];
                bbWriter.Write((int)bytes.Length);
                bbWriter.Write(bytes);
            }

            bb.Position = 0;
            return bb;
        }

        public override void Write(long value)
        {
            MaybeGrow();
            _binaryWriter.Write(value);
        }

        public override void Write(bool fieldValue)
        {
            MaybeGrow();
            _binaryWriter.Write((byte)(fieldValue ? 1 : 0));
        }

        public override void Write(sbyte fieldValue)
        {
            MaybeGrow();
            _binaryWriter.Write(fieldValue);
        }

        public override void Write(char fieldValue)
        {
            MaybeGrow();
            // java expects 2 bytes always (utf16/unicode) so we write as a ushort
            _binaryWriter.Write((ushort)fieldValue);
        }

        public override void Write(float fieldValue)
        {
            MaybeGrow();
            _binaryWriter.Write(fieldValue);
        }

        public override void Write(double fieldValue)
        {
            MaybeGrow();
            _binaryWriter.Write(fieldValue);
        }

        public override void Write(int fieldValue)
        {
            MaybeGrow();
            _binaryWriter.Write(fieldValue);
        }

        public override void Write(short value)
        {
            MaybeGrow();
            _binaryWriter.Write(value);
        }

        private void MaybeGrow()
        {
            // we never write more than 8 bytes at a time
            if (_memoryStream.Capacity - _memoryStream.Length < 8)
            {
                _memoryStream.SetLength(_memoryStream.Length * 2);
            }
        }

        protected override void Append(string s)
        {
            //strangely enough, we do nothing, and wait until we actually are asked to write the whole thing out
        }

        protected override string GetObjectTypeSignature(object o)
        {
            return _serializer.GetSerializationSignature(o.GetType());
        }

        protected override void Serialize(object o, string s)
        {
            _serializer.Serialize(this, o, s);
        }
    }
}
