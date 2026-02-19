/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using System.Linq;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class MergeTablesOperation
    {
        public static void Invoke(TableState parentState, TableStateBuilder childBuilder, string keyColumn,
            TableState[] mergeStates)
        {
            var allStates = new[] {parentState}.Concat(mergeStates).ToArray();
            var allHandles = allStates.Select(s => s.TableHandle).ToArray();
            parentState.Context.InvokeServerForItd(allStates, childBuilder,
                (ws, sa, fa) => ws.MergeTablesAsync(allHandles, childBuilder.TableHandle, keyColumn, sa, fa, fa));
        }
    }
}
