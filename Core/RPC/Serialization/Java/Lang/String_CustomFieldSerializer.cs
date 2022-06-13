/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using Deephaven.OpenAPI.Core.RPC.Serialization.API;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Lang
{
    public class String_CustomFieldSerializer
    {
        public static void Deserialize(ISerializationStreamReader streamReader, string instance)
        {
            // No fields
        }

        public static string Instantiate(ISerializationStreamReader streamReader)
        {
            return streamReader.ReadString();
        }

        public static void Serialize(ISerializationStreamWriter streamWriter, string instance)
        {
            streamWriter.Write(instance);
        }
    }
}
