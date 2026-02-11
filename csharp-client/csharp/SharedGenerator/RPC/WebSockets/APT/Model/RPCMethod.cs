/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.SharedGenerator.RPC.WebSockets.APT.Model
{
    public class RpcMethod
    {
        public string Name { get; set; }
        public RpcMethodParameter[] Parameters { get; set; }
        public RpcMethodCallback Callback { get; set; }
    }
}
