/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Core.RPC.Serialization.API;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Lang
{
    public class Double_CustomFieldSerializer
    {
        public static void Deserialize(ISerializationStreamReader streamReader, double? instance)
        {
            // No fields
        }

        public static double? Instantiate(ISerializationStreamReader streamReader)
        {
            return streamReader.ReadDouble();
        }

        public static void Serialize(ISerializationStreamWriter streamWriter, double? instance)
        {
            streamWriter.Write(instance.Value);
        }
    }
}
