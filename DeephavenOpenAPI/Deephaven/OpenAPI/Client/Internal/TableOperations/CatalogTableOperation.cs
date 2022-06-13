/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class CatalogTableOperation
    {
        public static void Invoke(ServerContext context, TableStateBuilder childBuilder)
        {
            context.InvokeServerForItd(Array.Empty<TableState>(), childBuilder,
                (ws, sa, fa) => ws.GetCatalogTableAsync(childBuilder.TableHandle, sa, fa, fa));
        }
    }
}
