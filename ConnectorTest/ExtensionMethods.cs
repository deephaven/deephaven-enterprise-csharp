/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace ConnectorTest
{
    /// <summary>
    /// Handy methods for testing
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Get a portion of an array
        /// </summary>
        /// <typeparam name="T">The source array type.</typeparam>
        /// <param name="data">The source array.</param>
        /// <param name="index">Source index to start sub array.</param>
        /// <param name="length">Length of subarray to return.</param>
        /// <returns>A subarray of the specified length.</returns>
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
