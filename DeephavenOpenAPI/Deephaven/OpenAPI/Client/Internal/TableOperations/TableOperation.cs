/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using Deephaven.OpenAPI.Shared.Batch;

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class BatchUtil
    {
        public static void InvokeBatchOperation(TableState parentState, TableStateBuilder childBuilder,
            BatchTableRequest.BatchTableRequestSerializedTableOps op)
        {
            parentState.Context.InvokeServerForBtr(new[] {parentState}, childBuilder, op);
        }
    }
}
