/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Core.RPC.Serialization.API;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Lang
{
    /// <summary>
    /// Custom serializer for nullable char
    /// </summary>
    public class Character_CustomFieldSerializer
    {
        public static void Deserialize(ISerializationStreamReader streamReader, char? instance)
        {
            // No fields
        }

        public static char? Instantiate(ISerializationStreamReader streamReader)
        {
            return streamReader.ReadChar();
        }

        public static void Serialize(ISerializationStreamWriter streamWriter, char? instance)
        {
            streamWriter.Write(instance.Value);
        }
    }
}
