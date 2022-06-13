/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class LazyUpdateOperation
    {
        public static void Invoke(TableState parentState, TableStateBuilder childBuilder, string[] columnSpecs)
        {
            parentState.Context.InvokeServerForItd(new[] {parentState}, childBuilder,
                (ws, sa, fa) => ws.LazyUpdateAsync(parentState.TableHandle, childBuilder.TableHandle, columnSpecs,
                    sa, fa, fa));
        }
    }
}
