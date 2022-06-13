/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class FetchHistoricalTableOperation
    {
        public static void Invoke(ServerContext context, TableStateBuilder childBuilder, string ns, string tableName)
        {
            context.InvokeServerForItd(Array.Empty<TableState>(), childBuilder,
                (ws, sa, fa) => ws.HistoricalTableAsync(childBuilder.TableHandle, ns, tableName, sa, fa, fa));
        }
    }
}
