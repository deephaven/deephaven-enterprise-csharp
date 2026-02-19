/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Cmd;

namespace Deephaven.OpenAPI.Shared.Ide.Cmd
{
    public class ClientScriptIdFactory : AbstractClientIdFactory<ScriptHandle>
    {
        public ClientScriptIdFactory(int clientConnectionId, int serverConnectionId) :
            base(clientConnectionId, serverConnectionId)
        {
            ScriptHandle_CustomFieldSerializer.RegisterIdFactory(clientConnectionId, this);
        }

        // we are pooling handles here, it is deprecated so others get their handles from us.
        // consider moving this class into the same package to package-protect the constructor, or moving TableHandle here.
        protected override ScriptHandle CreateHandle(int id)
        {
            return new ScriptHandle(id, _clientConnectionId);
        }
    }
}
