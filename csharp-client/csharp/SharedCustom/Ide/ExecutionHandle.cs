/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Cmd;

namespace Deephaven.OpenAPI.Shared.Ide
{
    public class ExecutionHandle : ServerObjectHandle
    {
        public int ClientId { get; }
        public int ConnectionId { get; }
        public int ScriptId { get; set; }

        public ExecutionHandle(int clientId, int clientConnectionId)
        {
            ClientId = clientId;
            ConnectionId = clientConnectionId;
            ScriptId = Uninitialized;
        }

        public override int GetClientId()
        {
            return ClientId;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj == null || this.GetType() != obj.GetType())
                return false;
            var that = (ExecutionHandle)obj;
            return ClientId == that.ClientId;
        }

        public override int GetHashCode()
        {
            return ClientId;
        }
    }
}
