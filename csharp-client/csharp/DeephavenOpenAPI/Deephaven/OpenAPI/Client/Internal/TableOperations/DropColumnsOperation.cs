/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Batch;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class DropColumnsOperation
    {
        public static void Invoke(TableState parentState, TableStateBuilder childBuilder, string[] columns)
        {
            var op = new BatchTableRequest.BatchTableRequestSerializedTableOps
            {
                Handles = new HandleMapping(parentState.TableHandle, childBuilder.TableHandle),
                DropColumns = columns,
            };
            BatchUtil.InvokeBatchOperation(parentState, childBuilder, op);
        }
    }
}
