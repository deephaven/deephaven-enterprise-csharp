/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Numerics;
using Deephaven.OpenAPI.Core.RPC.Serialization.API;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.Java.Math
{
    public class BigInteger_CustomFieldSerializer
    {
        public static void Deserialize(ISerializationStreamReader streamReader, BigInteger? instance)
        {
            // No fields
        }

        /// <summary>
        /// For future reference when GWT-RPC uses binary format instead of string for encoding BigInteger
        /// </summary>
        /// <param name="unscaledBytes"></param>
        /// <returns></returns>
        private static BigInteger Decode(byte[] unscaledBytes)
        {
            // java unscaled value is Big Endian, BigInteger expects Little Endian
            Array.Reverse(unscaledBytes);
            return new BigInteger(unscaledBytes);
        }

        public static BigInteger? Instantiate(ISerializationStreamReader streamReader)
        {
            int unscaledSize = streamReader.ReadInt32();
            byte[] unscaledBytes = new byte[unscaledSize];
            for (int i = 0; i < unscaledSize; i++)
            {
                unscaledBytes[i] = (byte)streamReader.ReadSByte();
            }
            Array.Reverse(unscaledBytes);
            return new BigInteger(unscaledBytes);
        }

        public static void Serialize(ISerializationStreamWriter streamWriter, BigInteger? instance)
        {
            if (instance.HasValue)
            {
                byte[] unscaled = instance.Value.ToByteArray();
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
