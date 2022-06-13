/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;

namespace Deephaven.OpenAPI.SharedGenerator.SerializableTypes
{
    /// <summary>
    /// A simple "serializable type" representing primitive types, so we can treat primitives more or less like other types during codegen.
    /// </summary>
    public class PrimitiveSerializableType : SerializableType
    {
        private readonly Type _type;

        public PrimitiveSerializableType(Type type) : base(type.FullName)
        {
            _type = type;
        }

        public override string GetStreamReaderMethodName()
        {
            if (_type == typeof(object))
            {
                return "ReadObject";
            }
            else
            {
                return "Read" + _type.Name;
            }
        }

        public override  string GetStreamWriterMethodName()
        {
            if (_type == typeof(object))
            {
                return "WriteObject";
            }
            else
            {
                return "Write";
            }
        }

        public const string BooleanTypeId = "boolean";
        public const string ByteTypeId = "byte";
        public const string ShortTypeId = "short";
        public const string IntTypeId = "int";
        public const string LongTypeId = "long";
        public const string CharTypeId = "char";
        public const string FloatTypeId = "float";
        public const string DoubleTypeId = "double";
        public const string StringTypeId = "string";
        public const string VoidTypeId = "void";
        public const string ObjectTypeId = "java.lang.Object";

        public static readonly Dictionary<string, SerializableType> PrimitiveTypes =
            new Dictionary<string, SerializableType>
            {
                { BooleanTypeId, new PrimitiveSerializableType(typeof(bool)) },
                { ByteTypeId, new PrimitiveSerializableType(typeof(sbyte)) },
                { ShortTypeId, new PrimitiveSerializableType(typeof(short)) },
                { IntTypeId, new PrimitiveSerializableType(typeof(int)) },
                { LongTypeId, new PrimitiveSerializableType(typeof(long)) },
                { CharTypeId, new PrimitiveSerializableType(typeof(char)) },
                { FloatTypeId, new PrimitiveSerializableType(typeof(float)) },
                { DoubleTypeId, new PrimitiveSerializableType(typeof(double)) },
                { StringTypeId, new PrimitiveSerializableType(typeof(string)) },
                { VoidTypeId, new PrimitiveSerializableType(typeof(Core.RPC.Serialization.Java.Lang.Void)) },
                { ObjectTypeId, new PrimitiveSerializableType(typeof(object)) }
            };
    }
}
