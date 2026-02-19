/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.SharedGenerator.RPC.WebSockets.APT.Model
{
    public class RpcEndpoint
    {
        public string EndpointPackage { get; set; }
        public string EndpointInterface { get; set; }
        public RpcMethod[] Methods { get; set; }
        public string EndpointHash { get; set; }
    }
}
