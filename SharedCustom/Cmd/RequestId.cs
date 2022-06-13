/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Shared.Cmd
{
    public class RequestId : ServerObjectHandle
    {
        public int ClientId { get; set; }

        public RequestId() { }

        public RequestId(int clientId)
        {
             ClientId = clientId;
        }

        public override int GetClientId()
        {
            return ClientId;
        }

        public override bool Equals(Object o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;

            var requestId = (RequestId)o;

            return ClientId == requestId.ClientId;
        }

        public override int GetHashCode()
        {
            return ClientId;
        }
    }
}
