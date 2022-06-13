/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class UngroupOperation
    {
        public static void Invoke(TableState parentState, TableStateBuilder childBuilder, bool nullFill,
            string[] columns)
        {
            parentState.Context.InvokeServerForItd(new[] {parentState}, childBuilder,
                (ws, sa, fa) => ws.UngroupAsync(parentState.TableHandle, childBuilder.TableHandle, nullFill, columns,
                    sa, fa, fa));
        }
    }
}
