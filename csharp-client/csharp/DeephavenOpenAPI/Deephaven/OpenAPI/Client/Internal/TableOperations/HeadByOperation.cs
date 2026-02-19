/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class HeadByOperation
    {
        public static void Invoke(TableState parentState, TableStateBuilder childBuilder, long nRows,
            string[] groupByColumnSpecs)
        {
            parentState.Context.InvokeServerForItd(new[] {parentState}, childBuilder,
                (ws, sa, fa) => ws.HeadByAsync(parentState.TableHandle, childBuilder.TableHandle, nRows,
                    groupByColumnSpecs, sa, fa, fa));
        }
    }
}
