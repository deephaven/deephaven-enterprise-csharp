/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Core.RPC.Serialization.API;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Lang
{
    public class Integer_CustomFieldSerializer
    {
        public static void Deserialize(ISerializationStreamReader streamReader, int? instance)
        {
            // No fields
        }

        public static int? Instantiate(ISerializationStreamReader streamReader)
        {
            return streamReader.ReadInt32();
        }

        public static void Serialize(ISerializationStreamWriter streamWriter, int? instance)
        {
            streamWriter.Write(instance.Value);
        }
    }
}
