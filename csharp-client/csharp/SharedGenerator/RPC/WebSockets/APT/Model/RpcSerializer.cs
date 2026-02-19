/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System.Collections.Generic;

namespace Deephaven.OpenAPI.SharedGenerator.RPC.WebSockets.APT.Model
{
    public class RpcSerializer
    {
        public string SerializerPackage { get; set; }

        public string SerializerInterface { get; set; }

        public Dictionary<string, RpcSerializableType> SerializableTypes { get; set; }

        public string SerializerHash { get; set; }
    }
}
