/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using Deephaven.OpenAPI.Core.RPC.Serialization.API;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Lang
{
    public class Void_CustomFieldSerializer
    {
        public static void Deserialize(ISerializationStreamReader streamReader, Void? instance)
        {
            // No fields
        }

        public static Void? Instantiate(ISerializationStreamReader streamReader)
        {
            return null;
        }

        public static void Serialize(ISerializationStreamWriter streamWriter, Void? instance)
        {
        }
    }
}
