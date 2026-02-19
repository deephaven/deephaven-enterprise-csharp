/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Core.RPC.Serialization.API;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Lang
{
    /// <summary>
    /// Custom serializer for nullable booleans
    /// </summary>
    public class Boolean_CustomFieldSerializer
    {
        public static void Deserialize(ISerializationStreamReader streamReader, bool? instance)
        {
            // No fields
        }

        public static bool? Instantiate(ISerializationStreamReader streamReader)
        {
            return streamReader.ReadBoolean();
        }

        public static void Serialize(ISerializationStreamWriter streamWriter, bool? instance)
        {
            streamWriter.Write(instance.Value);
        }
    }
}
