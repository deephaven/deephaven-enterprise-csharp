/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class FetchEmptyTableOperation
    {
        public static void Invoke(ServerContext context, TableStateBuilder childBuilder, long size,
            string[] columnNames, string[] columnTypes)
        {
            context.InvokeServerForItd(Array.Empty<TableState>(), childBuilder,
                (ws, sa, fa) => ws.EmptyTableAsync(childBuilder.TableHandle, size, columnNames, columnTypes, sa, fa, fa));
        }
    }
}
