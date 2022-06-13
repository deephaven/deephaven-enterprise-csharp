/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using Deephaven.OpenAPI.Core.RPC.Serialization.API;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Lang
{
    public class Long_CustomFieldSerializer
    {
        public static void Deserialize(ISerializationStreamReader streamReader, long? instance)
        {
            // No fields
        }

        public static long? Instantiate(ISerializationStreamReader streamReader)
        {
            return streamReader.ReadInt64();
        }

        public static void Serialize(ISerializationStreamWriter streamWriter, long? instance)
        {
            streamWriter.Write(instance.Value);
        }
    }
}
