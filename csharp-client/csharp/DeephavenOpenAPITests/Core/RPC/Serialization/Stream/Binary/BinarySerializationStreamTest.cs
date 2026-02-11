/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.IO;
using Deephaven.OpenAPI.Core.RPC.Serialization.API;
using Deephaven.OpenAPI.Core.RPC.Serialization.API.Impl;
using Deephaven.OpenAPI.Core.RPC.Serialization.Stream.Binary;
using NUnit.Framework;

namespace Deephaven.OpenAPI.CoreTests.RPC.Serialization.Stream.Binary
{
    [TestFixture]
    public class BinarySerializationStreamTest
    {
        private class CustomObject
        {
            public int _a;
            public short _b;
            public long _c;

            public CustomObject()
            {
            }

            public CustomObject(int a, short b, long c)
            {
                _a = a;
                _b = b;
                _c = c;
            }

            public override bool Equals(object o)
            {
                CustomObject co = (CustomObject)o;
                if (co == null)
                {
                    return false;
                }
                return co._a == _a && co._b == _b && co._c == _c;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private class NestedCustomObject
        {
            public bool _aBool;
            public CustomObject _customObject;

            public NestedCustomObject()
            {
            }

            public NestedCustomObject(bool aBool, CustomObject customObject)
            {
                _aBool = aBool;
                _customObject = customObject;
            }

            public override bool Equals(object obj)
            {
                NestedCustomObject nestedCustomObject = (NestedCustomObject) obj;
                if (nestedCustomObject == null)
                {
                    return false;
                }
                if ((nestedCustomObject._customObject == null && _customObject != null) ||
                    (nestedCustomObject._customObject != null && _customObject == null))
                {
                    return false;
                }

                return nestedCustomObject._aBool == _aBool
                       && (nestedCustomObject._customObject == _customObject || nestedCustomObject._customObject.Equals(_customObject));
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }


        private class CustomTypeFieldSerializer : FieldSerializer
        {
            public override void Deserial(ISerializationStreamReader reader, object instance)
            {
                CustomObject obj = (CustomObject)instance;
                obj._a = reader.ReadInt32();
                obj._b = reader.ReadInt16();
                obj._c = reader.ReadInt64();
            }

            public override void Serial(ISerializationStreamWriter writer, object instance)
            {
                CustomObject obj = (CustomObject) instance;
                writer.Write(obj._a);
                writer.Write(obj._b);
                writer.Write(obj._c);
            }

            public override object Create(ISerializationStreamReader reader)
            {
                return new CustomObject();
            }
        }

        private class NestedCustomTypeFieldSerializer : FieldSerializer
        {
            public override void Deserial(ISerializationStreamReader reader, object instance)
            {
                NestedCustomObject obj = (NestedCustomObject)instance;
                obj._aBool = reader.ReadBoolean();
                obj._customObject = (CustomObject)reader.ReadObject();
            }

            public override void Serial(ISerializationStreamWriter writer, object instance)
            {
                NestedCustomObject obj = (NestedCustomObject) instance;
                writer.Write(obj._aBool);
                writer.WriteObject(obj._customObject);
            }

            public override object Create(ISerializationStreamReader reader)
            {
                return new NestedCustomObject();
            }
        }

        private class BitSetFieldSerializer : FieldSerializer
        {
            public override void Deserial(ISerializationStreamReader reader, object instance)
            {
                NestedCustomObject obj = (NestedCustomObject)instance;
                obj._aBool = reader.ReadBoolean();
                obj._customObject = (CustomObject)reader.ReadObject();
            }

            public override void Serial(ISerializationStreamWriter writer, object instance)
            {
                NestedCustomObject obj = (NestedCustomObject)instance;
                writer.Write(obj._aBool);
                writer.WriteObject(obj._customObject);
            }

            public override object Create(ISerializationStreamReader reader)
            {
                return new NestedCustomObject();
            }
        }

        private class TypeSerializer : TypeSerializerImpl
        {
            private readonly Dictionary<string, FieldSerializer> _fieldSerializers = new Dictionary<string, FieldSerializer>();

            public TypeSerializer()
            {
                _fieldSerializers.Add(typeof(CustomObject).FullName, new CustomTypeFieldSerializer());
                _fieldSerializers.Add(typeof(NestedCustomObject).FullName, new NestedCustomTypeFieldSerializer());
            }
            protected override FieldSerializer Serializer(string name)
            {
                return _fieldSerializers[name];
            }

            public override string GetSerializationSignature(Type type)
            {
                // we just use the FullName but in real life need a map because the server uses java type names
                return type.FullName;
            }
        }

        private readonly TypeSerializer _typeSerializer = new TypeSerializer();

        private BinarySerializationStreamWriter GetStreamWriter()
        {
            BinarySerializationStreamWriter writer = new BinarySerializationStreamWriter(_typeSerializer);
            writer.SetFlags(0);
            return writer;
        }

        private BinarySerializationStreamReader GetSplitPayloadStreamReader(BinarySerializationStreamWriter writer)
        {
            MemoryStream payloadBytes = writer.GetPayloadBytes();
            string[] stringTable = writer.GetFinishedStringTable();
            BinarySerializationStreamReader reader =
                new BinarySerializationStreamReader(_typeSerializer, payloadBytes, stringTable);
            Assert.AreEqual(7, reader.GetVersion());
            Assert.AreEqual(0, reader.GetFlags());
            return reader;
        }

        private BinarySerializationStreamReader GetSinglePayloadStreamReader(
            BinarySerializationStreamWriter writer)
        {
            BinarySerializationStreamReader reader = new BinarySerializationStreamReader(_typeSerializer, writer.GetFullPayload());
            Assert.AreEqual(7, reader.GetVersion());
            Assert.AreEqual(0, reader.GetFlags());
            return reader;
        }

        [Test]
        public void TestBoolean()
        {
            BinarySerializationStreamWriter writer = GetStreamWriter();
            writer.Write(true);
            writer.Write(false);

            BinarySerializationStreamReader reader = GetSplitPayloadStreamReader(writer);

            Assert.AreEqual(true, reader.ReadBoolean());
            Assert.AreEqual(false, reader.ReadBoolean());
        }

        [Test]
        public void TestByte()
        {
            BinarySerializationStreamWriter writer = GetStreamWriter();
            writer.Write((sbyte) 4);
            writer.Write((sbyte) 1);
            writer.Write(SByte.MaxValue);
            writer.Write(SByte.MinValue);
            writer.Write((sbyte) 1);

            BinarySerializationStreamReader reader = GetSplitPayloadStreamReader(writer);

            Assert.AreEqual(4, reader.ReadSByte());
            Assert.AreEqual(1, reader.ReadSByte());
            Assert.AreEqual(SByte.MaxValue, reader.ReadSByte());
            Assert.AreEqual(SByte.MinValue, reader.ReadSByte());
            Assert.AreEqual(1, reader.ReadSByte());
        }

        [Test]
        public void TestChar()
        {
            BinarySerializationStreamWriter writer = GetStreamWriter();
            writer.Write('A');
            writer.Write('B');
            writer.Write(Char.MaxValue);
            writer.Write(Char.MinValue);
            writer.Write('B');

            BinarySerializationStreamReader reader = GetSplitPayloadStreamReader(writer);

            Assert.AreEqual('A', reader.ReadChar());
            Assert.AreEqual('B', reader.ReadChar());
            Assert.AreEqual(Char.MaxValue, reader.ReadChar());
            Assert.AreEqual(Char.MinValue, reader.ReadChar());
            Assert.AreEqual('B', reader.ReadChar());
        }

        [Test]
        public void TestInt16()
        {
            BinarySerializationStreamWriter writer = GetStreamWriter();
            writer.Write((short) 4);
            writer.Write((short) 1);
            writer.Write(Int16.MaxValue);
            writer.Write(Int16.MinValue);
            writer.Write((short) 1);

            BinarySerializationStreamReader reader = GetSplitPayloadStreamReader(writer);

            Assert.AreEqual(4, reader.ReadInt16());
            Assert.AreEqual(1, reader.ReadInt16());
            Assert.AreEqual(Int16.MaxValue, reader.ReadInt16());
            Assert.AreEqual(Int16.MinValue, reader.ReadInt16());
            Assert.AreEqual(1, reader.ReadInt16());
        }

        [Test]
        public void TestInt32()
        {
            BinarySerializationStreamWriter writer = GetStreamWriter();
            writer.Write(4);
            writer.Write(1);
            writer.Write(Int32.MaxValue);
            writer.Write(Int32.MinValue);
            writer.Write(1);

            BinarySerializationStreamReader reader = GetSplitPayloadStreamReader(writer);

            Assert.AreEqual(4, reader.ReadInt32());
            Assert.AreEqual(1, reader.ReadInt32());
            Assert.AreEqual(Int32.MaxValue, reader.ReadInt32());
            Assert.AreEqual(Int32.MinValue, reader.ReadInt32());
            Assert.AreEqual(1, reader.ReadInt32());
        }

        [Test]
        public void TestInt64()
        {
            BinarySerializationStreamWriter writer = GetStreamWriter();
            writer.Write(4L);
            writer.Write(1L);
            writer.Write(Int64.MaxValue);
            writer.Write(Int64.MinValue);
            writer.Write(1L);
            writer.Write(Int64.MaxValue);
            writer.Write(Int64.MinValue);

            BinarySerializationStreamReader reader = GetSplitPayloadStreamReader(writer);

            Assert.AreEqual(4L, reader.ReadInt64());
            Assert.AreEqual(1L, reader.ReadInt64());
            Assert.AreEqual(Int64.MaxValue, reader.ReadInt64());
            Assert.AreEqual(Int64.MinValue, reader.ReadInt64());
            Assert.AreEqual(1L, reader.ReadInt64());
            Assert.AreEqual(Int64.MaxValue, reader.ReadInt64());
            Assert.AreEqual(Int64.MinValue, reader.ReadInt64());
        }

        [Test]
        public void TestSingle()
        {
            BinarySerializationStreamWriter writer = GetStreamWriter();

            writer.Write(1.23f);
            writer.Write((float) 3);
            writer.Write((float) 4);
            writer.Write(1.23f);
            writer.Write(Single.MaxValue);
            writer.Write(Single.MinValue);
            writer.Write(Single.Epsilon);
            writer.Write(Single.NaN);
            writer.Write(Single.NegativeInfinity);
            writer.Write(Single.PositiveInfinity);

            BinarySerializationStreamReader reader = GetSplitPayloadStreamReader(writer);

            Assert.AreEqual(1.23f, reader.ReadSingle(), 0);
            Assert.AreEqual(3.0f, reader.ReadSingle(), 0);
            Assert.AreEqual(4.0f, reader.ReadSingle(), 0);
            Assert.AreEqual(1.23f, reader.ReadSingle(), 0);
            Assert.AreEqual(Single.MaxValue, reader.ReadSingle(), 0);
            Assert.AreEqual(Single.MinValue, reader.ReadSingle(), 0);
            Assert.AreEqual(Single.Epsilon, reader.ReadSingle(), 0);
            Assert.True(Single.IsNaN(reader.ReadSingle()));
            Assert.AreEqual(Single.NegativeInfinity, reader.ReadSingle(), 0);
            Assert.AreEqual(Single.PositiveInfinity, reader.ReadSingle(), 0);

        }

        [Test]
        public void TestDouble()
        {
            BinarySerializationStreamWriter writer = GetStreamWriter();

            writer.Write(1.23);
            writer.Write((double) 3);
            writer.Write((double) 4);
            writer.Write(1.23);
            writer.Write(Double.MaxValue);
            writer.Write(Double.MinValue);
            writer.Write(Double.Epsilon);
            writer.Write(Double.NaN);
            writer.Write(Double.NegativeInfinity);
            writer.Write(Double.PositiveInfinity);

            BinarySerializationStreamReader reader = GetSplitPayloadStreamReader(writer);

            Assert.AreEqual(1.23, reader.ReadDouble(), 0);
            Assert.AreEqual(3.0, reader.ReadDouble(), 0);
            Assert.AreEqual(4.0, reader.ReadDouble(), 0);
            Assert.AreEqual(1.23, reader.ReadDouble(), 0);
            Assert.AreEqual(Double.MaxValue, reader.ReadDouble(), 0);
            Assert.AreEqual(Double.MinValue, reader.ReadDouble(), 0);
            Assert.AreEqual(Double.Epsilon, reader.ReadDouble(), 0);
            Assert.True(Double.IsNaN(reader.ReadDouble()));
            Assert.AreEqual(Double.NegativeInfinity, reader.ReadDouble(), 0);
            Assert.AreEqual(Double.PositiveInfinity, reader.ReadDouble(), 0);

        }

        //just the one test with split payloads for primitives, since no primitives use strings
        [Test]
        public void TestSinglePayloadInt()
        {
            BinarySerializationStreamWriter writer = GetStreamWriter();
            writer.Write(4);
            writer.Write(1);
            writer.Write(Int32.MaxValue);
            writer.Write(Int32.MinValue);
            writer.Write(1);

            BinarySerializationStreamReader reader = GetSinglePayloadStreamReader(writer);

            Assert.AreEqual(4, reader.ReadInt32());
            Assert.AreEqual(1, reader.ReadInt32());
            Assert.AreEqual(Int32.MaxValue, reader.ReadInt32());
            Assert.AreEqual(Int32.MinValue, reader.ReadInt32());
            Assert.AreEqual(1, reader.ReadInt32());
        }

        [Test]
        public void TestString()
        {
            BinarySerializationStreamWriter writer = GetStreamWriter();

            writer.Write("foo");
            writer.Write("foo1");
            writer.Write("foo");
            writer.Write("bar");

            BinarySerializationStreamReader reader = GetSplitPayloadStreamReader(writer);

            Assert.AreEqual("foo", reader.ReadString());
            Assert.AreEqual("foo1", reader.ReadString());
            Assert.AreEqual("foo", reader.ReadString());
            Assert.AreEqual("bar", reader.ReadString());
        }

        [Test]
        public void TestNullString()
        {
            BinarySerializationStreamWriter writer = GetStreamWriter();

            writer.Write(null);

            BinarySerializationStreamReader reader = GetSplitPayloadStreamReader(writer);

            object v = reader.ReadString();
            Assert.IsNull(v);
        }

        [Test]
        public void TestSinglePayloadString()
        {
            BinarySerializationStreamWriter writer = GetStreamWriter();

            writer.Write("foo");
            writer.Write("foo1");
            writer.Write("foo");
            writer.Write("bar");

            BinarySerializationStreamReader reader = GetSinglePayloadStreamReader(writer);

            Assert.AreEqual("foo", reader.ReadString());
            Assert.AreEqual("foo1", reader.ReadString());
            Assert.AreEqual("foo", reader.ReadString());
            Assert.AreEqual("bar", reader.ReadString());
        }

        [Test]
        public void TestLargePayload()
        {
            BinarySerializationStreamWriter writer = GetStreamWriter();
            // write an amount more than the buffer size and not an even multiple
            int n = (int)(BinarySerializationStreamWriter.InitBufSize * Math.PI);
            for (int i = 0; i < n; i++)
            {
                writer.Write(i);
            }

            BinarySerializationStreamReader reader = GetSinglePayloadStreamReader(writer);

            for (int i = 0; i < n; i++)
            {
                Assert.AreEqual(i, reader.ReadInt32());
            }
        }

        [Test]
        public void TestObject()
        {
            var object1 = new CustomObject(1, 2, 3);
            var object2 = new CustomObject(4, 5, 6);
            BinarySerializationStreamWriter writer = GetStreamWriter();

            writer.WriteObject(object1);
            writer.WriteObject(object2);
            writer.WriteObject(object1);

            BinarySerializationStreamReader reader = GetSinglePayloadStreamReader(writer);

            var o1 = reader.ReadObject();
            Assert.IsTrue(object1.Equals(o1));
            var o2 = reader.ReadObject();
            Assert.IsTrue(object2.Equals(o2));
            var o3 = reader.ReadObject();
            Assert.IsTrue(object1.Equals(o3));
        }

        [Test]
        public void TestNullObject()
        {
            var object1 = new CustomObject(1, 2, 3);
            var object2 = new CustomObject(4, 5, 6);
            BinarySerializationStreamWriter writer = GetStreamWriter();

            writer.WriteObject(object1);
            writer.WriteObject(null);
            writer.WriteObject(object2);
            writer.WriteObject(null);
            writer.WriteObject(object1);

            BinarySerializationStreamReader reader = GetSinglePayloadStreamReader(writer);

            var o1 = reader.ReadObject();
            Assert.IsTrue(object1.Equals(o1));
            var n = reader.ReadObject();
            Assert.IsNull(n);
            var o2 = reader.ReadObject();
            Assert.IsTrue(object2.Equals(o2));
            var n2 = reader.ReadObject();
            Assert.IsNull(n2);
            var o3 = reader.ReadObject();
            Assert.IsTrue(object1.Equals(o3));
        }

        [Test]
        public void TestNestedObject()
        {
            var obj = new CustomObject(1, 2, 3);
            var nestedObject = new NestedCustomObject(true, obj);
            var nestedObject2 = new NestedCustomObject(true, null);
            BinarySerializationStreamWriter writer = GetStreamWriter();

            writer.WriteObject(obj);
            writer.WriteObject(nestedObject);
            writer.WriteObject(obj);
            writer.WriteObject(nestedObject2);

            BinarySerializationStreamReader reader = GetSinglePayloadStreamReader(writer);

            var o1 = reader.ReadObject();
            Assert.IsTrue(obj.Equals(o1));
            var o2 = reader.ReadObject();
            Assert.IsTrue(nestedObject.Equals(o2));
            var o3 = reader.ReadObject();
            Assert.IsTrue(obj.Equals(o3));
            var o4 = reader.ReadObject();
            Assert.IsTrue(nestedObject2.Equals(o4));
        }
    }
}

