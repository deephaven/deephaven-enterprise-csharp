/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Core.RPC.Serialization.API;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Lang
{
    public class Short_CustomFieldSerializer
    {
        public static void Deserialize(ISerializationStreamReader streamReader, short? instance)
        {
            // No fields
        }

        public static short? Instantiate(ISerializationStreamReader streamReader)
        {
            return streamReader.ReadInt16();
        }

        public static void Serialize(ISerializationStreamWriter streamWriter, short? instance)
        {
            streamWriter.Write(instance.Value);
        }
    }
}
