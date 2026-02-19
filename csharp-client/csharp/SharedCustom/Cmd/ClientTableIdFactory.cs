/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Shared.Cmd
{
    public class ClientTableIdFactory : AbstractClientIdFactory<TableHandle>
    {
        public ClientTableIdFactory(int clientConnectionId, int serverConnectionId) :
            base(clientConnectionId, serverConnectionId)
        {
            TableHandle_CustomFieldSerializer.RegisterIdFactory(clientConnectionId, this);
        }

        protected override TableHandle CreateHandle(int id)
        {
            return new TableHandle(id, _clientConnectionId);
        }
    }
}
