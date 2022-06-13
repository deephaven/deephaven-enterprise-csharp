/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Core.RPC.Serialization.API
{
    /// <summary>
    /// Thrown when an error occurs within the RPC (de)serialization process.
    /// </summary>
    public class SerializationException : Exception
    {
        public SerializationException(string msg) : base(msg) { }

        public SerializationException(string msg, Exception cause) : base(msg, cause) { }

        public SerializationException(Exception cause) : base(null, cause) { }
    }
}
