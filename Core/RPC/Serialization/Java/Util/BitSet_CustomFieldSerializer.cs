/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections;
using Deephaven.OpenAPI.Core.RPC.Serialization.API;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Util
{
    /// <summary>
    /// Custom serializer to handle java BitSet (mapped to the BitArray type in .NET)
    /// </summary>
    public class BitSet_CustomFieldSerializer
    {
        public static void Deserialize(ISerializationStreamReader streamReader, BitArray instance)
        {
            // already done
        }

        public static BitArray Instantiate(ISerializationStreamReader streamReader)
        {
            var count = streamReader.ReadInt32();
            var bitArray = new BitArray(count); // we know it'll be at least this long but probably still will have to grow
            for (var i = 0; i < count; i++)
            {
                var ix = streamReader.ReadInt32();
                bitArray.Length = System.Math.Max(ix+1, bitArray.Length); // grow the bit array if needed, this doesn't happen automatically
                bitArray.Set(ix, true);
            }
            return bitArray;
        }

        public static void Serialize(ISerializationStreamWriter streamWriter, BitArray instance)
        {
            var cardinality = 0;
            for (var i = 0; i < instance.Length; i++)
            {
                if (instance.Get(i))
                {
                    cardinality++;
                }
            }
            streamWriter.Write(cardinality);
            for (var i = 0; i < instance.Length; i++)
            {
                if (instance.Get(i))
                {
                    streamWriter.Write(i);
                }
            }
        }
    }
}
