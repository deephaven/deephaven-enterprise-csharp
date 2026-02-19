/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;

namespace Deephaven.OpenAPI.Core.API.Util
{
    public static class ArrayUtil
    {
        public static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }

        public static int GetHashCode<T>(T[] a)
        {
            int hashCode = 0;
            for(var i = 0; a != null && i < a.Length; i++)
            {
                hashCode = hashCode * 31 + a[i].GetHashCode();
            }
            return hashCode;
        }

        public static string ToString<T>(T[] a)
        {
            if (a == null)
                return "null";
            return "[" + string.Join(",", a) + "]";
        }

        public static byte[] SignedBytesToUnsignedBytes(sbyte[] src)
        {
            var result = new byte[src.Length];
            for (var i = 0; i < src.Length; ++i)
            {
                result[i] = (byte)src[i];
            }

            return result;
        }

        public static sbyte[] UnsignedBytesToSignedBytes(byte[] src)
        {
            var result = new sbyte[src.Length];
            for (var i = 0; i < src.Length; ++i)
            {
                result[i] = (sbyte)src[i];
            }

            return result;
        }
    }
}
