/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class FetchTimeTableOperation
    {
        public static void Invoke(ServerContext context, TableStateBuilder childBuilder, long startTimeNanos,
            long periodNanos)
        {
            context.InvokeServerForItd(Array.Empty<TableState>(), childBuilder,
                (ws, sa, fa) => ws.TimeTableAsync(childBuilder.TableHandle, startTimeNanos, periodNanos, sa, fa, fa));
        }
    }
}
