/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class FetchNewTableOperation
    {
        public static void Invoke(ServerContext context, TableStateBuilder childBuilder, ColumnHolder[] columnHolders)
        {
            context.InvokeServerForItd(Array.Empty<TableState>(), childBuilder,
                (ws, sa, fa) => ws.NewTableAsync(childBuilder.TableHandle, columnHolders, sa, fa, fa));
        }
    }
}
