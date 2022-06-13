/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.IO;
using System.Text;
using Deephaven.OpenAPI.Core.RPC.Serialization.API;
using Deephaven.OpenAPI.Core.RPC.Serialization.API.Impl;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Stream.Binary
{
    public class BinarySerializationStreamReader : AbstractSerializationStreamReader
    {
        private readonly ITypeSerializer _serializer;
        private readonly MemoryStream _bb;
        private readonly BinaryReader _payload;
        private readonly string[] _strings;

        private int _claimedTokens;

        public BinarySerializationStreamReader(ITypeSerializer serializer, MemoryStream bb, string[] strings)
        {
            _serializer = serializer;
            _bb = bb;
            _payload = new BinaryReader(bb);
            _strings = strings;
            int version = _payload.ReadInt32();
            int flags = _payload.ReadInt32();
            int length = _payload.ReadInt32();

            if(length != bb.Length - bb.Position)
            {
                throw new ArgumentException("Unexpected buffer length");
            }

            SetVersion(version);
            SetFlags(flags);
        }

        private const int StrTableOffset = sizeof(int) * 3; // 3 ints of header

        public BinarySerializationStreamReader(ITypeSerializer serializer, MemoryStream bb)
        {
            _serializer = serializer;
            _bb = bb;
            _payload = new BinaryReader(bb);
            int version = _payload.ReadInt32();
            int flags = _payload.ReadInt32();
            int length = _payload.ReadInt32();
            SetVersion(version);
            SetFlags(flags);

            //strings are in the payload, read them out first and assign them
            string[] payloadStrings = new string[0];
            // see if there is a stringCount, and thus strings present
            int strTablePosition = StrTableOffset + length;
            if (bb.Length > strTablePosition)
            {
                bb.Position = strTablePosition;
                int stringsCount = _payload.ReadInt32();
                if (stringsCount < 1)
                {
                    throw new ArgumentException("Invalid string count in payload: " + stringsCount);
                }
                bb.Position = StrTableOffset + sizeof(int) + length; // 3 ints of header + 1 count
                // ensure there is enough space for at least that many string lengths left
                if (bb.Length - bb.Position < (stringsCount * sizeof(int)))
                {
                    throw new ArgumentException("Payload claims to have " + stringsCount + " strings, but only has space left for " + (RemainingBytes >> 2));
                }
                payloadStrings = new string[stringsCount];
                for (int i = 0; i < stringsCount; i++)
                {
                    int stringLength = _payload.ReadInt32();
                    if (bb.Length - bb.Position < stringLength)
                    {
                        throw new ArgumentException("Payload claims to have a string with length " + stringLength + " but only " + RemainingBytes + " bytes remain");
                    }
                    byte[] bytes = _payload.ReadBytes(stringLength);
                    payloadStrings[i] = Encoding.UTF8.GetString(bytes); // is this right or should we use UTF8?
                }
            }

            // move back to the starting point, right after the three headers
            bb.Position = StrTableOffset;

            // move the limit of the payload to just before strings start (if any)
            bb.SetLength(strTablePosition);

            _strings = payloadStrings;
        }

        private long RemainingBytes => _bb.Length - _bb.Position;

        protected override object Deserialize(string typeSignature)
        {
            var id = ReserveDecodedObjectIndex();
            var instance = _serializer.Instantiate(this, typeSignature);
            RememberDecodedObject(id, instance);
            _serializer.Deserialize(this, instance, typeSignature);
            return instance;
        }

        protected override string GetString(int index)
        {
            return index > 0 ? _strings[index - 1] : null;
        }

        public override bool ReadBoolean()
        {
            return _payload.ReadByte() == 1;//or zero
        }

        public override sbyte ReadSByte()
        {
            return (sbyte)_payload.ReadByte();
        }

        public override char ReadChar()
        {
            // java always sends 2 bytes so we read as ushort
            return (char)_payload.ReadUInt16();
        }

        public override double ReadDouble()
        {
            return _payload.ReadDouble();
        }

        public override float ReadSingle()
        {
            return _payload.ReadSingle();
        }

        public override int ReadInt32()
        {
            return _payload.ReadInt32();
        }

        public override long ReadInt64()
        {
            return _payload.ReadInt64();
        }

        public override short ReadInt16()
        {
            return _payload.ReadInt16();
        }

        public override string ReadString()
        {
            return GetString(ReadInt32());
        }

        public override void ClaimItems(int slots)
        {
            long limit = _bb.Length << 0;
            if (_claimedTokens + slots > limit + _strings.Length)
            {
                throw new SerializationException("Request claims to be larger than it is");
            }
            _claimedTokens += slots;
        }
    }
}
