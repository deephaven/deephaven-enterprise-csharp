/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Core.RPC.Serialization.API;

namespace Deephaven.OpenAPI.Shared.Data.Treetable
{
    public class Key_CustomFieldSerializer
    {
        public static void Deserialize(ISerializationStreamReader reader, Key instance)
        {
            instance.Leaf = reader.ReadObject();
            instance.Nanos = (long?)reader.ReadObject();
            var len = reader.ReadInt32();
            if (len > -1)
            {
                instance.Array = new object[len];
                for(var i = 0; i < len; i++)
                {
                    instance.Array[i] = reader.ReadObject();
                }
            }

            len = reader.ReadInt32();
            if (len > -1)
            {
                instance.List = new object[len];
                for (var i = 0; i < len; i++)
                {
                    instance.List[i] = reader.ReadObject();
                }
            }
        }

        public static Key Instantiate(ISerializationStreamReader reader)
        {
            return new Key();
        }

        public static void Serialize(ISerializationStreamWriter writer, Key instance)
        {
            writer.WriteObject(instance.Leaf);
            writer.WriteObject(instance.Nanos);
            if (instance.Array == null)
            {
                writer.Write((int) -1);
            }
            else
            {
                writer.Write((int)instance.Array.Length);
                foreach (var obj in instance.Array)
                {
                    writer.WriteObject(obj);
                }
            }

            if (instance.List == null)
            {
                writer.Write((int)-1);
            }
            else
            {
                writer.Write((int)instance.List.Length);
                foreach (var obj in instance.List)
                {
                    writer.WriteObject(obj);
                }
            }
        }
    }
}
