/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Numerics;
using Deephaven.OpenAPI.Core.RPC.Serialization.API;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Math
{
    public class BigDecimal_CustomFieldSerializer
    {
        public static void Deserialize(ISerializationStreamReader streamReader, BigDecimal? instance)
        {
            // No fields
        }

        private static readonly BigInteger BigInteger10 = new BigInteger(10);

        public static BigDecimal? Instantiate(ISerializationStreamReader streamReader)
        {
            // read the raw data
            int scale = streamReader.ReadInt32();
            int unscaledSize = streamReader.ReadInt32();
            byte[] unscaledBytes = new byte[unscaledSize];
            for(int i = 0; i < unscaledSize; i++)
            {
                unscaledBytes[i] = (byte)streamReader.ReadSByte();
            }

            // java unscaled value is Big Endian, BigInteger expects Little Endian
            Array.Reverse(unscaledBytes);

            // use .NET BigInteger math to get an unscaled absolute value
            BigInteger unscaled = new BigInteger(unscaledBytes);
            if (scale < 0)
            {
                unscaled *= BigInteger.Pow(BigInteger10, -scale);
                scale = 0;
            }

            return new BigDecimal(unscaled, scale);
        }

        public static void Serialize(ISerializationStreamWriter streamWriter, BigDecimal? instance)
        {
            if (instance.HasValue)
            {
                streamWriter.Write(instance.Value.Scale);
                byte[] unscaled = instance.Value.Unscaled.ToByteArray();
                Array.Reverse(unscaled);
                streamWriter.Write(unscaled.Length);
                for (int i = 0; i < unscaled.Length; i++)
                {
                    streamWriter.Write((sbyte)unscaled[i]);
                }
            }
        }
    }
}
