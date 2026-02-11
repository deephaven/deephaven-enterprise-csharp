/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using Deephaven.OpenAPI.Core.RPC.Serialization.API;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Lang
{
    /// <summary>
    /// Custom serializer for nullable byte
    /// </summary>
    public class Byte_CustomFieldSerializer
    {
        public static void Deserialize(ISerializationStreamReader streamReader, sbyte? instance)
        {
            // No fields.
        }

        public static sbyte? Instantiate(ISerializationStreamReader streamReader)
        {
            return streamReader.ReadSByte();
        }

        public static void Serialize(ISerializationStreamWriter streamWriter, sbyte? instance)
        {
            streamWriter.Write(instance.Value);
        }
    }
}
