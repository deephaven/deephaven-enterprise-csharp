/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Cmd;

namespace Deephaven.OpenAPI.Shared.Ide.Cmd
{
    public class ClientExecutionIdFactory : AbstractClientIdFactory<ExecutionHandle>
    {
        public ClientExecutionIdFactory(int clientConnectionId, int serverConnectionId) :
            base(clientConnectionId, serverConnectionId)
        {
            ExecutionHandle_CustomFieldSerializer.RegisterIdFactory(clientConnectionId, this);
        }

        // we are pooling handles here, it is deprecated so others get their handles from us.
        // consider moving this class into the same package to package-protect the constructor, or moving TableHandle here.
        protected override ExecutionHandle CreateHandle(int id)
        {
            return new ExecutionHandle(id, _clientConnectionId);
        }
    }
}
