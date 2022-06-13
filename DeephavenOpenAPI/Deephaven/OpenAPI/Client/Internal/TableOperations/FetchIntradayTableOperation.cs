/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class FetchIntradayTableOperation
    {
        public static void Invoke(ServerContext context, TableStateBuilder childBuilder, string ns, string tableName,
            string internalPartition, bool live)
        {
            context.InvokeServerForItd(Array.Empty<TableState>(), childBuilder,
                (ws, sa, fa) => ws.IntradayTableAsync(childBuilder.TableHandle, ns, tableName, internalPartition, live,
                    sa, fa, fa));
        }
    }
}
