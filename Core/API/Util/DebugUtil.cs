/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Threading;

namespace Deephaven.OpenAPI.Core.API.Util
{
    /// <summary>
    /// This Deephaven "Debug Utilities" class contains some simple facilities to help with debugging.
    /// </summary>
    public static class DebugUtil
    {
        /// <summary>
        /// Prints <paramref name="text"/> prefixed by the current thread ID.
        /// </summary>
        /// <param name="text">The message to print</param>
        public static void Print(string text)
        {
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}]: {text}");
        }
    }
}
