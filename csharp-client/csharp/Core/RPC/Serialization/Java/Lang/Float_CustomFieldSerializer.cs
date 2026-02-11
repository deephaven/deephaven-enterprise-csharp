/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using Deephaven.OpenAPI.Core.RPC.Serialization.API;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Lang
{
    public class Float_CustomFieldSerializer
    {
        public static void Deserialize(ISerializationStreamReader streamReader, float? instance)
        {
            // No fields
        }

        public static float? Instantiate(ISerializationStreamReader streamReader)
        {
            return streamReader.ReadSingle();
        }

        public static void Serialize(ISerializationStreamWriter streamWriter, float? instance)
        {
            streamWriter.Write(instance.Value);
        }
    }
}
