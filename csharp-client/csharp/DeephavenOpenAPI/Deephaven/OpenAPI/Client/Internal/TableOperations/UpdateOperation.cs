/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class UpdateOperation
    {
        public static void Invoke(TableState parentState, TableStateBuilder childBuilder, string[] columnSpecs)
        {
            parentState.Context.InvokeServerForItd(new[] {parentState}, childBuilder,
                (ws, sa, fa) => ws.UpdateAsync(parentState.TableHandle, childBuilder.TableHandle, columnSpecs, sa, fa, fa));
        }
    }
}
